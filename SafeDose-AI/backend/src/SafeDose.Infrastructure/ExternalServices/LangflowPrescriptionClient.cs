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
        // 1. Parse FlowUrl to extract base URL and Flow ID
        var uri = new Uri(_flowUrl);
        var baseUrl = uri.GetLeftPart(UriPartial.Authority);
        var flowId = uri.Segments.Last().Replace("?stream=false", "").Trim('/');
        
        var uploadUrl = $"{baseUrl}/api/v1/files/upload/{flowId}";

        // 2. Upload the file to Langflow using multipart/form-data
        using var multipartContent = new MultipartFormDataContent();
        
        // FastAPI (which Langflow uses) is strict and sometimes fails if the boundary has quotes around it.
        var boundary = multipartContent.Headers.ContentType?.Parameters.FirstOrDefault(o => o.Name == "boundary");
        if (boundary != null && boundary.Value != null)
        {
            boundary.Value = boundary.Value.Replace("\"", "");
        }

        var streamContent = new StreamContent(imageStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        multipartContent.Add(streamContent, "file", fileName);

        using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        uploadRequest.Headers.Add("x-api-key", _apiKey);
        uploadRequest.Content = multipartContent;

        var uploadResponse = await _httpClient.SendAsync(uploadRequest);
        if (!uploadResponse.IsSuccessStatusCode)
        {
            var err = await uploadResponse.Content.ReadAsStringAsync();
            throw new Exception($"Failed to upload file to Langflow. Status: {uploadResponse.StatusCode}, Error: {err}");
        }

        var uploadResultStr = await uploadResponse.Content.ReadAsStringAsync();
        var uploadResultJson = JsonNode.Parse(uploadResultStr);
        var serverFilePath = uploadResultJson?["file_path"]?.ToString();

        if (string.IsNullOrEmpty(serverFilePath))
        {
            throw new Exception($"File uploaded successfully but 'file_path' was not returned. Response: {uploadResultStr}");
        }

        // 3. Send to Langflow using the returned file path
        return await ParsePrescriptionByUrlAsync(serverFilePath);
    }

    public async Task<ParsedPrescriptionDto> ParsePrescriptionByUrlAsync(string imageUrlOrPath)
    {
        // Generate a unique session ID per request.
        var sessionId = Guid.NewGuid().ToString();

        var requestBody = new
        {
            input_value = "Extract the prescription data from the attached image.",
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
            
            // Temporary debug output to see the full raw text from Gemini
            Console.WriteLine("=== RAW LANGFLOW TEXT ===");
            Console.WriteLine(textResult);
            Console.WriteLine("=========================");

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