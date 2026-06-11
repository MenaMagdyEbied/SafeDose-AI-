using Microsoft.Extensions.Configuration;
using SafeDose.Application.DTOs.PrescriptionDTOs;
using SafeDose.Application.Interfaces;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SafeDose.Infrastructure.ExternalServices;

public class LangflowPrescriptionClient : ILangflowPrescriptionClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _flowUrl;

    public LangflowPrescriptionClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["LangflowPrescription:ApiKey"]
            ?? throw new InvalidOperationException("LangflowPrescription ApiKey is missing");
        _flowUrl = configuration["LangflowPrescription:FlowUrl"]
            ?? throw new InvalidOperationException("LangflowPrescription FlowUrl is missing");
    }

    public async Task<ParsedPrescriptionDto> ParsePrescriptionAsync(Stream imageStream, string fileName, string contentType)
    {
        // 1. Extract flowId and baseAddress from _flowUrl
        var uri = new Uri(_flowUrl);
        var baseAddress = $"{uri.Scheme}://{uri.Authority}";
        var flowId = uri.Segments.Last().Trim('/');

        // 2. Read the imageStream to a byte array so we can send it
        byte[] imageBytes;
        if (imageStream is MemoryStream ms)
        {
            imageBytes = ms.ToArray();
        }
        else
        {
            using var tempMs = new MemoryStream();
            await imageStream.CopyToAsync(tempMs);
            imageBytes = tempMs.ToArray();
        }

        // 3. Upload file to Langflow using multipart/form-data
        // We try the standard `/api/v1/files/upload/{flowId}` endpoint first,
        // and fallback to `/api/v1/upload/{flowId}` if it returns 404 or other errors.
        string filePath = null;
        var uploadUrls = new[]
        {
            $"{baseAddress}/api/v1/files/upload/{flowId}",
            $"{baseAddress}/api/v1/upload/{flowId}"
        };

        HttpResponseMessage uploadResponse = null;
        string uploadResponseContent = null;

        foreach (var uploadUrl in uploadUrls)
        {
            using var formData = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            formData.Add(fileContent, "file", fileName);

            using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            uploadRequest.Headers.Add("x-api-key", _apiKey);
            uploadRequest.Content = formData;

            try
            {
                uploadResponse = await _httpClient.SendAsync(uploadRequest);
                if (uploadResponse.IsSuccessStatusCode)
                {
                    uploadResponseContent = await uploadResponse.Content.ReadAsStringAsync();
                    break;
                }
            }
            catch
            {
                // Ignore and try the fallback URL
            }
        }

        if (uploadResponse == null || !uploadResponse.IsSuccessStatusCode)
        {
            var errorMsg = uploadResponse != null
                ? await uploadResponse.Content.ReadAsStringAsync()
                : "Unknown error during file upload";
            throw new HttpRequestException($"Failed to upload prescription image to Langflow. Status: {uploadResponse?.StatusCode}, Error: {errorMsg}");
        }

        // 4. Parse upload response to get file_path
        using var responseDoc = JsonDocument.Parse(uploadResponseContent);
        var root = responseDoc.RootElement;
        if (root.TryGetProperty("file_path", out var filePathProp))
        {
            filePath = filePathProp.GetString();
        }
        else
        {
            throw new InvalidOperationException($"Upload response did not contain 'file_path'. Response: {uploadResponseContent}");
        }

        // 5. Run the flow passing the file_path as the input_value
        using var runRequest = new HttpRequestMessage(HttpMethod.Post, _flowUrl);
        runRequest.Headers.Add("x-api-key", _apiKey);

        var runRequestBody = new
        {
            input_value = filePath,
            input_type = "chat",
            output_type = "chat",
            tweaks = new { }
        };

        runRequest.Content = JsonContent.Create(runRequestBody);

        var runResponse = await _httpClient.SendAsync(runRequest);
        runResponse.EnsureSuccessStatusCode();

        var runResponseContent = await runResponse.Content.ReadAsStringAsync();
        return MapLangflowResponseToDto(runResponseContent);
    }

    public async Task<ParsedPrescriptionDto> ParsePrescriptionByUrlAsync(string imageUrl)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _flowUrl);
        request.Headers.Add("x-api-key", _apiKey);

        var requestBody = new
        {
            input_value = imageUrl,
            input_type = "chat",
            output_type = "chat",
            tweaks = new { }
        };

        request.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return MapLangflowResponseToDto(responseContent);
    }

    private ParsedPrescriptionDto MapLangflowResponseToDto(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;
        var dto = new ParsedPrescriptionDto();

        try
        {
            var textResult = string.Empty;
            if (root.TryGetProperty("outputs", out var outputs) && outputs.GetArrayLength() > 0)
            {
                var firstOutput = outputs[0];
                if (firstOutput.TryGetProperty("outputs", out var innerOutputs) && innerOutputs.GetArrayLength() > 0)
                {
                    var firstInnerOutput = innerOutputs[0];
                    if (firstInnerOutput.TryGetProperty("results", out var results))
                    {
                        if (results.TryGetProperty("message", out var message) && message.TryGetProperty("text", out var text))
                        {
                            textResult = text.GetString();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(textResult))
            {
                var parsedData = JsonSerializer.Deserialize<ParsedPrescriptionDto>(textResult, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (parsedData != null)
                {
                    return parsedData;
                }
            }
        }
        catch
        {
        }

        return dto;
    }
}