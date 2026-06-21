using SafeDose.Application.DTOs.Chatbot;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases.Chatbot;

// End-to-end chat handler for ANONYMOUS users (no login).
// Cannot do symptom analysis (no patient context to know their meds).
// Only answers general drug-info questions using the Egyptian drug catalog
// (CSV-seeded into SQL via DrugCatalogSeeder).
public class ProcessPublicChatMessageUseCase
{
    private readonly IDrugRepository _drugs;
    private readonly IChatLlmClient  _llm;

    public ProcessPublicChatMessageUseCase(IDrugRepository drugs, IChatLlmClient llm)
    {
        _drugs = drugs;
        _llm   = llm;
    }

    public async Task<ChatResponseDto> ExecuteAsync(
        PublicChatRequestDto request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            throw new ArgumentException("Message cannot be empty");

        var catalogHits  = await _drugs.SearchCatalogAsync(request.Message, limit: 5);
        var catalogText  = ProcessChatMessageUseCase.FormatCatalog(catalogHits);
        var systemPrompt = ChatPrompts.BuildPublicPrompt(catalogText);

        var llmResult = await _llm.CompleteAsync(systemPrompt, request.Message, ct);
        return new ChatResponseDto(
            Reply:            llmResult.Content,
            Intent:           ChatPrompts.DetectIntentFromReply(llmResult.Content),
            PromptTokens:     llmResult.PromptTokens,
            CompletionTokens: llmResult.CompletionTokens);
    }
}
