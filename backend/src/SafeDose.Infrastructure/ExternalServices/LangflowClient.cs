using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SafeDose.Application.Interfaces;

namespace SafeDose.Infrastructure.ExternalServices;

// Calls the Langflow Drug Interaction pipeline.
// Reads config from "LangflowDrugInteraction" section.
public class LangflowClient : ILangflowClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LangflowClient> _logger;
    private readonly string _flowUrl;
    private readonly string _apiKey;

    public LangflowClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<LangflowClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _flowUrl = configuration["LangflowDrugInteraction:FlowUrl"]
            ?? throw new InvalidOperationException(
                "LangflowDrugInteraction:FlowUrl not configured in appsettings");
        _apiKey = configuration["LangflowDrugInteraction:ApiKey"] ?? string.Empty;

        if (!string.IsNullOrEmpty(_apiKey)
            && !_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        }
    }

    public async Task<LangflowInteractionResult?> CheckMultiDrugInteractionAsync(
        LangflowInteractionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                input_value = JsonSerializer.Serialize(request),
                output_type = "text",
                input_type = "chat"
            };

            var response = await _httpClient.PostAsJsonAsync(_flowUrl, payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Langflow returned {Status}: {Body}",
                    response.StatusCode,
                    errorBody[..Math.Min(300, errorBody.Length)]);
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

    // Output path: outputs[0].outputs[0].results.message.text
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
