using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.UseCases;

// On-demand interaction check that takes catalog drug IDs (not saved Drug rows).
// Used by the "افحص التداخلات الدوائية الآن" page.
// If a PatientId is passed, the patient's verified active meds + allergies
// are folded into the context sent to Langflow.
public class CheckCatalogInteractionsUseCase
{
    private readonly IDrugRepository _drugs;
    private readonly IPatientRepository _patients;
    private readonly IPatientMedicationProvider _patientMeds;
    private readonly ILangflowClient _langflow;

    public CheckCatalogInteractionsUseCase(
        IDrugRepository drugs,
        IPatientRepository patients,
        IPatientMedicationProvider patientMeds,
        ILangflowClient langflow)
    {
        _drugs = drugs;
        _patients = patients;
        _patientMeds = patientMeds;
        _langflow = langflow;
    }

    public async Task<CheckInteractionsResponseDto> ExecuteAsync(
        CheckCatalogInteractionsRequestDto request,
        string? accountId,
        CancellationToken cancellationToken = default)
    {
        if (request.DrugCatalogIds is null || request.DrugCatalogIds.Length == 0)
            throw new ArgumentException("At least one drug must be selected");
        if (request.DrugCatalogIds.Length > 6)
            throw new ArgumentException("Maximum 6 drugs per check (UI limit)");

        // Load the catalog entries for the picked drugs.
        var catalogEntries = new List<LangflowDrugInput>();
        foreach (var id in request.DrugCatalogIds.Distinct())
        {
            var entry = await _drugs.GetCatalogByIdAsync(id);
            if (entry == null)
                throw new ArgumentException($"Catalog drug {id} not found");

            catalogEntries.Add(new LangflowDrugInput(
                DrugId: entry.DrugCatalogId,
                DrugName: entry.CommercialNameEn,
                ScientificName: entry.ScientificName,
                DrugClass: entry.DrugClass
            ));
        }

        // Optional patient context (age, allergies, current verified meds).
        LangflowPatientContext? patientContext = null;
        if (request.PatientId.HasValue)
        {
            var patient = await _patients.GetByIdAsync(request.PatientId.Value)
                ?? throw new ArgumentException("Patient not found");

            if (!string.IsNullOrWhiteSpace(accountId) &&
                !string.Equals(patient.AccountId, accountId, StringComparison.Ordinal))
                throw new UnauthorizedAccessException("This patient does not belong to you");

            var activeMeds = await _patientMeds.GetActiveMedicationsForPatientAsync(
                patient.PatientId, cancellationToken);

            patientContext = new LangflowPatientContext(
                Age: CalcAge(patient.DateOfBirth),
                Gender: GenderLabel(patient.Gender),
                ChronicConditions: SplitTags(patient.ChronicConditions),
                Allergies: SplitTags(patient.Allergies),
                CurrentMedications: activeMeds
                    .Select(m => new LangflowDrugInput(
                        m.DrugId,
                        m.DrugName,
                        m.ScientificName,
                        null))
                    .ToArray()
            );
        }

        // Run the check.
        var result = await _langflow.CheckMultiDrugInteractionAsync(
            new LangflowInteractionRequest(catalogEntries.ToArray(), patientContext),
            cancellationToken);

        // Langflow failure - return precautionary "caution" verdict.
        if (result == null)
        {
            return new CheckInteractionsResponseDto(
                InteractionCheckId: 0,
                Level: InteractionLevel.Caution,
                LabelArabic: "احذر",
                Color: "#FFA000",
                TitleArabic: "تعذر التحقق من التداخلات",
                ExplanationArabic: "خدمة الفحص غير متاحة حالياً. يُرجى المحاولة لاحقاً أو استشارة الصيدلي.",
                RecommendedActionArabic: "راجع طبيبك أو الصيدلي قبل تناول الأدوية معاً",
                AnalyzedDrugs: Array.Empty<AnalyzedDrugDto>(),
                ConflictingPairs: Array.Empty<ConflictingPairDto>(),
                Sources: Array.Empty<string>(),
                SafetyDisclaimerArabic: "استشر طبيبك أو الصيدلي",
                CheckedAt: DateTime.UtcNow
            );
        }

        return new CheckInteractionsResponseDto(
            InteractionCheckId: 0,
            Level: result.Level,
            LabelArabic: result.LabelArabic,
            Color: ColorFor(result.Level),
            TitleArabic: result.TitleArabic,
            ExplanationArabic: result.ExplanationArabic,
            RecommendedActionArabic: result.RecommendedActionArabic,
            AnalyzedDrugs: result.AnalyzedDrugs
                .Select(d => new AnalyzedDrugDto(d.DrugId, d.ArabicName, d.EnglishName, d.DosageNote, d.Role))
                .ToArray(),
            ConflictingPairs: result.ConflictingPairs
                .Select(p => new ConflictingPairDto(p.DrugA, p.DrugB, p.ReasonArabic, p.Severity))
                .ToArray(),
            Sources: result.Sources,
            SafetyDisclaimerArabic: "استشر طبيبك أو الصيدلي",
            CheckedAt: DateTime.UtcNow
        );
    }

    private static int CalcAge(DateOnly? dob)
    {
        if (!dob.HasValue) return 0;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dob.Value.Year;
        if (dob.Value > today.AddYears(-age)) age--;
        return age < 0 ? 0 : age;
    }

    private static string GenderLabel(byte? g) => g switch
    {
        1 => "ذكر",
        2 => "أنثى",
        _ => "غير محدد"
    };

    private static string[] SplitTags(string? csv)
        => string.IsNullOrWhiteSpace(csv)
            ? Array.Empty<string>()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string ColorFor(InteractionLevel level) => level switch
    {
        InteractionLevel.Safe => "#4CAF50",
        InteractionLevel.Caution => "#FFA000",
        InteractionLevel.Danger => "#D32F2F",
        _ => "#9E9E9E"
    };
}
