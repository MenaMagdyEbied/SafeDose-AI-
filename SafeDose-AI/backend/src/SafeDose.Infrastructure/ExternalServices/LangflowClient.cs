using SafeDose.Application.Interfaces;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace SafeDose.Infrastructure.ExternalServices;

public class LangflowClient : ILangflowClient
{
    private readonly HttpClient _httpClient;
    private readonly string _drugInteractionFlowUrl;

    public LangflowClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _drugInteractionFlowUrl = configuration["Langflow:DrugInteractionUrl"]
            ?? throw new InvalidOperationException("Langflow URL not configured");
    }

    public async Task<InteractionCheckResponse> CheckDrugInteractionAsync(int patientId, IEnumerable<string> drugs)
    {
        var request = new
        {
            patient_id = patientId,
            drugs = drugs.ToArray()
        };

        var response = await _httpClient.PostAsJsonAsync(_drugInteractionFlowUrl, request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<InteractionCheckResponse>()
            ?? throw new InvalidOperationException("Empty response from Langflow");
    }
}
