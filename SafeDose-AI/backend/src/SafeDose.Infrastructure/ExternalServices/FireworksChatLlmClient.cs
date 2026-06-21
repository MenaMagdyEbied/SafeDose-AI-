using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SafeDose.Application.Interfaces;

namespace SafeDose.Infrastructure.ExternalServices;

public class FireworksChatLlmClient : IChatLlmClient
{
    private readonly HttpClient _http;
    private readonly ILogger<FireworksChatLlmClient> _log;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly double _temperature;

    public FireworksChatLlmClient(HttpClient http, IConfiguration cfg, ILogger<FireworksChatLlmClient> log)
    {
        _http = http;
        _log  = log;

        var section = cfg.GetSection("ChatbotLlm");
        _apiKey      = section["ApiKey"] ?? throw new InvalidOperationException("ChatbotLlm:ApiKey missing in appsettings");
        _model       = section["Model"]  ?? "accounts/fireworks/models/qwen3p6-plus";
        _temperature = double.TryParse(section["Temperature"], out var t) ? t : 0.02;

        var baseUrl = section["BaseUrl"] ?? "https://api.fireworks.ai/inference/v1/";
        if (!baseUrl.EndsWith("/")) baseUrl += "/";
        _http.BaseAddress = new Uri(baseUrl);

        _log.LogInformation(
            "FireworksChatLlmClient init. BaseUrl={Url} Model={Model} KeyPrefix={Prefix} KeyLen={Len}",
            baseUrl, _model,
            _apiKey.Length >= 6 ? _apiKey.Substring(0, 6) : _apiKey, _apiKey.Length);
    }

    public async Task<ChatLlmResponse> CompleteAsync(string systemPrompt, string userMessage, CancellationToken ct = default)
    {
        // Hand-built JSON — bypasses any naming policy / serializer quirks
        var sb = new StringBuilder();
        sb.Append("{\"model\":");        AppendStr(sb, _model);
        sb.Append(",\"temperature\":");  sb.Append(_temperature.ToString(System.Globalization.CultureInfo.InvariantCulture));
        sb.Append(",\"max_tokens\":4000");
        sb.Append(",\"messages\":[");
        sb.Append("{\"role\":\"system\",\"content\":"); AppendStr(sb, systemPrompt); sb.Append("},");
        sb.Append("{\"role\":\"user\",\"content\":");   AppendStr(sb, userMessage);  sb.Append("}");
        sb.Append("]}");
        var json = sb.ToString();

        using var req = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        _log.LogInformation("Calling Fireworks: POST {Uri}", new Uri(_http.BaseAddress!, "chat/completions"));

        var resp = await _http.SendAsync(req, ct);
        var raw  = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            _log.LogError("Fireworks {Status}: {Body}", (int)resp.StatusCode, raw);
            throw new HttpRequestException($"Fireworks call failed with status {(int)resp.StatusCode}: {raw}");
        }

        using var doc = JsonDocument.Parse(raw);
        var msg = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message");

        string content = "";
        if (msg.TryGetProperty("content", out var c) && c.ValueKind == JsonValueKind.String)
            content = c.GetString() ?? "";
        if (string.IsNullOrWhiteSpace(content)
            && msg.TryGetProperty("reasoning_content", out var rc) && rc.ValueKind == JsonValueKind.String)
            content = rc.GetString() ?? "";

        int prompt = 0, completion = 0;
        if (doc.RootElement.TryGetProperty("usage", out var usage))
        {
            if (usage.TryGetProperty("prompt_tokens", out var pt))     prompt     = pt.GetInt32();
            if (usage.TryGetProperty("completion_tokens", out var ctk)) completion = ctk.GetInt32();
        }

        return new ChatLlmResponse(content.Trim(), prompt, completion);
    }

    // JSON-escape a string and wrap in quotes (handles ", \, control chars, Arabic, emojis)
    private static void AppendStr(StringBuilder sb, string s)
    {
        sb.Append('"');
        foreach (var ch in s)
        {
            switch (ch)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"':  sb.Append("\\\""); break;
                case '\n': sb.Append("\\n");  break;
                case '\r': sb.Append("\\r");  break;
                case '\t': sb.Append("\\t");  break;
                case '\b': sb.Append("\\b");  break;
                case '\f': sb.Append("\\f");  break;
                default:
                    if (ch < ' ') sb.Append("\\u").Append(((int)ch).ToString("x4"));
                    else          sb.Append(ch);
                    break;
            }
        }
        sb.Append('"');
    }
}
