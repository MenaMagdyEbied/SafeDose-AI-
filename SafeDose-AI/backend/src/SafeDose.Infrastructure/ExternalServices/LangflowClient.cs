using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SafeDose.Application.Interfaces;

namespace SafeDose.Infrastructure.ExternalServices;

// Calls Mina's Langflow Drug Interaction pipeline.
// Retry policy (3x exponential backoff) is wired in DI via Polly — not here.
// This class just makes the call and parses the (nested) response.
//
// Configurable via appsettings.json:
//   "Langflow": {
//     "BaseUrl": "http://localhost:7860",
//     "DrugInteractionFlowId": "f81dafc2-4634-4f38-aed1-118f10ddc9f6",
//     "ApiKey": "sk-..."
//   }
public class LangflowClient : ILangflowClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LangflowClient> _logger;
    private readonly string _baseUrl;
    private readonly string _flowId;
    private readonly string _apiKey;

    public LangflowClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<LangflowClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _baseUrl = configuration["Langflow:BaseUrl"]
            ?? throw new InvalidOperationException("Langflow:BaseUrl not configured");
        _flowId = configuration["Langflow:DrugInteractionFlowId"]
            ?? throw new InvalidOperationException("Langflow:DrugInteractionFlowId not configured");
        _apiKey = configuration["Langflow:ApiKey"] ?? string.Empty;

        if (!string.IsNullOrEmpty(_apiKey) && !_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
    }

    public async Task<LangflowInteractionResult?> CheckMultiDrugInteractionAsync(
        LangflowInteractionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Langflow's /api/v1/run/{flowId}?stream=false expects:
            // {
            //   "input_value": "<our JSON as a string>",
            //   "output_type": "text",
            //   "input_type": "chat"
            // }
            var payload = new
            {
                input_value = JsonSerializer.Serialize(request),
                output_type = "text",
                input_type = "chat"
            };

            var url = $"{_baseUrl.TrimEnd('/')}/api/v1/run/{_flowId}?stream=false";
            var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Langflow returned {Status}: {Body}",
                    response.StatusCode, errorBody[..Math.Min(300, errorBody.Length)]);
                return null;
            }

            var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseLangflowResponse(rawJson);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Langflow request timed out");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Langflow request failed");
            return null;
        }
    }

    // Langflow wraps the actual agent output deep in its response.
    // Path: outputs[0].outputs[0].results.message.text
    private LangflowInteractionResult? ParseLangflowResponse(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var text = doc.RootElement
                .GetProperty("outputs")[0]
                .GetProperty("outputs")[0]
                .GetProperty("results")
                .GetProperty("message")
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text)) return null;

            // Strip markdown wrappers Gemini sometimes adds
            var clean = text.Trim()
                .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                .Replace("```", "")
                .Trim();

            return JsonSerializer.Deserialize<LangflowInteractionResult>(
                clean,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Langflow response");
            return null;
        }
    }
}
