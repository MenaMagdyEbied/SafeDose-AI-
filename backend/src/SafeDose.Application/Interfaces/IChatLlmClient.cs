namespace SafeDose.Application.Interfaces;

// Provider-agnostic chat completion. Backed by Fireworks today, can be swapped
// for OpenAI/Anthropic/Azure later without touching the use case.
public interface IChatLlmClient
{
    Task<ChatLlmResponse> CompleteAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken ct = default);
}

public record ChatLlmResponse(string Content, int PromptTokens, int CompletionTokens);
