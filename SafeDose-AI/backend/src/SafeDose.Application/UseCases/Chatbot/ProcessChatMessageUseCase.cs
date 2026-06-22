using System.Text;
using System.Text.Json;
using SafeDose.Application.DTOs;
using SafeDose.Application.DTOs.Chatbot;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases.Chatbot;

// End-to-end chat handler for AUTHENTICATED users with family-account-aware patient picking.
//
// Flow:
//   1) No PatientId AND zero patients on account  → tell user to add a patient first
//   2) No PatientId AND exactly one patient       → auto-use that patient, proceed
//   3) No PatientId AND multiple patients (family) → return AvailablePatients list,
//                                                     frontend shows picker, user re-asks with PatientId
//   4) PatientId provided                          → validate ownership, proceed
public class ProcessChatMessageUseCase
{
    private readonly GetChatbotPatientContextUseCase _patientContext;
    private readonly IPatientRepository              _patients;
    private readonly IDrugRepository                 _drugs;
    private readonly IChatLlmClient                  _llm;

    public ProcessChatMessageUseCase(
        GetChatbotPatientContextUseCase patientContext,
        IPatientRepository patients,
        IDrugRepository drugs,
        IChatLlmClient llm)
    {
        _patientContext = patientContext;
        _patients       = patients;
        _drugs          = drugs;
        _llm            = llm;
    }

    public async Task<ChatResponseDto> ExecuteAsync(
        string accountId,
        ChatRequestDto request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            throw new ArgumentException("Message cannot be empty");

        // --- Patient-selection flow for family accounts ---
        if (!request.PatientId.HasValue)
        {
            var familyPatients = await _patients.GetByAccountIdAsync(accountId);

            if (familyPatients.Count == 0)
            {
                return new ChatResponseDto(
                    Reply:            "محتاج تضيف مريض الأول في إعدادات الحساب عشان أقدر أساعدك بحالته الصحية.",
                    Intent:           "needs_patient_registration",
                    PromptTokens:     0,
                    CompletionTokens: 0);
            }

            if (familyPatients.Count > 1)
            {
                var options = familyPatients
                    .Select(p => new ChatPatientOptionDto(
                        PatientId: p.PatientId,
                        FullName:  p.FullName,
                        Age:       CalcAge(p.DateOfBirth),
                        Gender:    MapGender(p.Gender)))
                    .ToList();

                var names = string.Join("، ", options.Select(o => o.FullName));
                return new ChatResponseDto(
                    Reply:             $"حسابك عائلي وفيه أكتر من مريض ({names}). من فضلك اختار مين اللي بتسأل عنه عشان أقدر أساعدك صح.",
                    Intent:            "needs_patient_selection",
                    PromptTokens:      0,
                    CompletionTokens:  0,
                    AvailablePatients: options);
            }

            // Single patient → use them silently and proceed.
            request = request with { PatientId = familyPatients.First().PatientId };
        }

        // --- Normal flow: patient is known, ownership-checked by GetChatbotPatientContextUseCase ---
        var patient = await _patientContext.ExecuteAsync(accountId, request.PatientId, ct);
        if (patient == null)
            throw new UnauthorizedAccessException("Patient not found or not owned by this account");

        var catalogHits  = await _drugs.SearchCatalogAsync(request.Message, limit: 5);
        var patientJson  = JsonSerializer.Serialize(patient, JsonOpts);
        var systemPrompt = ChatPrompts.BuildAuthenticatedPrompt(patientJson, FormatCatalog(catalogHits));

        var llmResult = await _llm.CompleteAsync(systemPrompt, request.Message, ct);
        return new ChatResponseDto(
            Reply:            llmResult.Content,
            Intent:           ChatPrompts.DetectIntentFromReply(llmResult.Content),
            PromptTokens:     llmResult.PromptTokens,
            CompletionTokens: llmResult.CompletionTokens);
    }

    internal static string FormatCatalog(IReadOnlyList<DrugSearchResultDto> hits)
    {
        if (hits.Count == 0) return "(لا يوجد دواء مطابق في الكتالوج)";
        var sb = new StringBuilder();
        foreach (var h in hits)
        {
            sb.Append("- ");
            sb.Append(h.CommercialNameEn);
            if (!string.IsNullOrWhiteSpace(h.CommercialNameAr)) sb.Append($" / {h.CommercialNameAr}");
            if (!string.IsNullOrWhiteSpace(h.ScientificName))   sb.Append($" (المادة الفعالة: {h.ScientificName})");
            if (!string.IsNullOrWhiteSpace(h.DrugClass))        sb.Append($" - تصنيف: {h.DrugClass}");
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    private static int? CalcAge(DateOnly? dob)
    {
        if (dob == null) return null;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dob.Value.Year;
        if (today < dob.Value.AddYears(age)) age--;
        return age;
    }

    private static string? MapGender(byte? g) => g switch
    {
        1 => "ذكر",
        2 => "أنثى",
        _ => null,
    };

    internal static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        Encoder       = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}
