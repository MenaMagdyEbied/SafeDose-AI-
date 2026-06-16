using Microsoft.Extensions.Configuration;
using SafeDose.Application.DTOs.PrescriptionDTOs;
using SafeDose.Application.Interfaces;
using System.IO;
using System.Net.Http.Headers;
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
        // 1. Prepare base address and flow ID
        var uri = new Uri(_flowUrl);
        var baseAddress = $"{uri.Scheme}://{uri.Authority}";
        var flowId = uri.Segments.Last().Trim('/');

        // 2. Read the image into memory
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();

        // 3. Upload the image to Langflow
        var uploadUrl = $"{baseAddress}/api/v1/files/upload/{flowId}";
        var filePath = await UploadImageAsync(uploadUrl, imageBytes, fileName, contentType);

        // If the first upload URL fails, try the fallback URL
        if (string.IsNullOrEmpty(filePath))
        {
            var fallbackUrl = $"{baseAddress}/api/v1/upload/{flowId}";
            filePath = await UploadImageAsync(fallbackUrl, imageBytes, fileName, contentType);
        }

        if (string.IsNullOrEmpty(filePath))
        {
            throw new Exception("Failed to upload the image to Langflow.");
        }

        // 4. Ask Langflow to process the uploaded file
        return await ParsePrescriptionByUrlAsync(filePath);
    }

    public async Task<ParsedPrescriptionDto> ParsePrescriptionByUrlAsync(string imageUrlOrPath)
    {
        // 1. Prepare the request body
        // Langflow needs the image passed in a 'files' array for the Chat Input, not as plain text.
        var requestBody = new
        {
            input_value = "Extract the prescription data from the attached image.",
            input_type = "chat",
            output_type = "chat",
            tweaks = new Dictionary<string, object>
            {
                { "Chat Input", new { files = new[] { imageUrlOrPath } } }
            }
        };

        // 2. Send the request to Langflow
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

    private async Task<string> UploadImageAsync(string url, byte[] imageBytes, string fileName, string contentType)
    {
        try
        {
            using var formData = new MultipartFormDataContent();
            
            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            
            formData.Add(fileContent, "file", fileName);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-api-key", _apiKey);
            request.Content = formData;

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null; // Return null if upload fails

            // Parse response to get the file path
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonNode = JsonNode.Parse(responseContent);
            return jsonNode?["file_path"]?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private ParsedPrescriptionDto MapLangflowResponseToDto(string rawJson)
    {
        try
        {
            // 1. Parse the JSON response dynamically
            var jsonNode = JsonNode.Parse(rawJson);

            // 2. Go deep into the Langflow response structure to find the actual text message
            var textResult = jsonNode?["outputs"]?[0]?["outputs"]?[0]?["results"]?["message"]?["text"]?.ToString();

            if (string.IsNullOrEmpty(textResult))
            {
                throw new Exception($"Could not find the text message in Langflow response. Raw JSON: {rawJson}");
            }

            // 3. Clean up the text (Remove Markdown formatting like ```json and ```)
            var cleanJson = textResult
                .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                .Replace("```", "")
                .Trim();

            // 4. Convert the clean JSON text into our C# Object
            var dto = JsonSerializer.Deserialize<ParsedPrescriptionDto>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (dto == null || (dto.PrescriptionName == null && (dto.Drugs == null || dto.Drugs.Count == 0)))
            {
                throw new Exception($"DEBUG: Langflow returned empty DTO. Raw text from Langflow: {textResult}");
            }

            return dto ?? new ParsedPrescriptionDto();
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse Langflow JSON response. Error: {ex.Message}. Raw text: {rawJson}");
        }
    }
}