using Microsoft.Extensions.Configuration;
using SafeDose.Application.DTOs.PrescriptionDTOs;
using SafeDose.Application.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SafeDose.Infrastructure.ExternalServices;

public class LangflowPrescriptionClient : ILangflowPrescriptionClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _flowUrl;

    public LangflowPrescriptionClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["LangflowPrescription:ApiKey"] ?? throw new Exception("Langflow ApiKey is missing");
        _flowUrl = configuration["LangflowPrescription:FlowUrl"] ?? throw new Exception("Langflow FlowUrl is missing");
    }

    public async Task<ParsedPrescriptionDto> ParsePrescriptionAsync(Stream imageStream, string fileName, string contentType)
    {
        // 1. Read the image into memory
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();

        // 2. Convert image to base64 data URL.
        // This is the only confirmed-working method to get the image to Gemini via the Langflow API.
        // File-path-based upload does NOT pass the image correctly to the vision model.
        var base64 = Convert.ToBase64String(imageBytes);
        var dataUrl = $"data:{contentType};base64,{base64}";

        // 3. Send to Langflow with image embedded as base64
        return await ParsePrescriptionByUrlAsync(dataUrl);
    }

    public async Task<ParsedPrescriptionDto> ParsePrescriptionByUrlAsync(string imageUrlOrPath)
    {
        // Generate a unique session ID per request.
        // This prevents contamination from old Playground sessions stored in Langflow's chat history.
        var sessionId = Guid.NewGuid().ToString();

        var requestBody = new
        {
            input_value = "",          // Empty - exactly like the Playground (image only, no user text)
            input_type = "chat",
            output_type = "chat",
            session_id = sessionId,
            tweaks = new Dictionary<string, object>
            {
                {
                    "ChatInput-272Js", new
                    {
                        files = new[] { imageUrlOrPath }
                    }
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _flowUrl);
        request.Headers.Add("x-api-key", _apiKey);
        request.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Langflow API returned {(int)response.StatusCode} ({response.StatusCode}): {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        return MapLangflowResponseToDto(responseContent);
    }

    private ParsedPrescriptionDto MapLangflowResponseToDto(string rawJson)
    {
        try
        {
            var jsonNode = JsonNode.Parse(rawJson);

            var textResult = jsonNode?["outputs"]?[0]?["outputs"]?[0]?["results"]?["message"]?["text"]?.ToString();

            if (string.IsNullOrEmpty(textResult))
            {
                throw new Exception($"Could not find the text message in Langflow response. Raw JSON: {rawJson}");
            }

            var cleanJson = textResult
                .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                .Replace("```", "")
                .Trim();

            // Check if the AI returned an error response (e.g., image not recognized as a prescription)
            var parsedNode = JsonNode.Parse(cleanJson);
            var errorField = parsedNode?["error"]?.ToString();
            if (!string.IsNullOrEmpty(errorField))
            {
                throw new Exception($"AI could not process the image: {errorField}");
            }

            var dto = JsonSerializer.Deserialize<ParsedPrescriptionDto>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return dto ?? new ParsedPrescriptionDto();
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse Langflow JSON response. Error: {ex.Message}. Raw text: {rawJson}");
        }
    }
}