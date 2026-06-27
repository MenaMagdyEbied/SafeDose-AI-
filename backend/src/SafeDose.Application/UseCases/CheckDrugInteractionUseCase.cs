using System.Text.Json;
using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;
using SafeDose.Domain.Enums;
using SafeDose.Domain.Services;

namespace SafeDose.Application.UseCases;

// Orchestrates the drug interaction pipeline.
//
// Order:
//   1. Validate input (1-6 drugs, exist in catalog)
//   2. Detect duplicates
//   3. Cache lookup
//   4. HARD RULE: allergy match → Level 3 (no LLM needed)
//   5. HARD RULE: critical-pair table → Level 3 (no LLM needed)
//   6. Load patient context (allergies, conditions, current meds)
//   7. Call Langflow 4-stage pipeline
//   8. Graceful degradation if LLM fails → Level 2 precautionary
//   9. Combine signals via SeverityCalculator (final level)
//  10. Persist InteractionCheck with full audit trail
//  11. Write compliance audit log
//  12. Return mapped response for UI
public class CheckDrugInteractionUseCase
{
    private readonly IDrugRepository _drugRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IPatientMedicationProvider _patientMeds;
    private readonly IInteractionRepository _interactionRepository;
    private readonly ICriticalPairLookup _criticalPairLookup;
    private readonly ILangflowClient _langflowClient;
    private readonly IAuditLogService _audit;

    private readonly AllergyCrossReactivityMatcher _allergyMatcher;
    private readonly SeverityCalculator _severityCalc;
    private readonly DuplicateDrugDetector _duplicateDetector;
    private readonly CacheKeyHasher _cacheHasher;

    public CheckDrugInteractionUseCase(
        IDrugRepository drugRepository,
        IPatientRepository patientRepository,
        IPatientMedicationProvider patientMeds,
        IInteractionRepository interactionRepository,
        ICriticalPairLookup criticalPairLookup,
        ILangflowClient langflowClient,
        IAuditLogService audit,
        AllergyCrossReactivityMatcher allergyMatcher,
        SeverityCalculator severityCalc,
        DuplicateDrugDetector duplicateDetector,
        CacheKeyHasher cacheHasher)
    {
        _drugRepository = drugRepository;
        _patientRepository = patientRepository;
        _patientMeds = patientMeds;
        _interactionRepository = interactionRepository;
        _criticalPairLookup = criticalPairLookup;
        _langflowClient = langflowClient;
        _audit = audit;
        _allergyMatcher = allergyMatcher;
        _severityCalc = severityCalc;
        _duplicateDetector = duplicateDetector;
        _cacheHasher = cacheHasher;
    }

    public async Task<CheckInteractionsResponseDto> ExecuteAsync(
        CheckInteractionsRequestDto request,
        CancellationToken cancellationToken = default,
        string? accountId = null)
    {
        // INPUT VALIDATION
        if (request.DrugIds is null || request.DrugIds.Length == 0)
            throw new ArgumentException("At least one drug must be selected");
        if (request.DrugIds.Length > 6)
            throw new ArgumentException("Maximum 6 drugs per check (UI limit)");

        var distinctIds = request.DrugIds.Distinct().ToArray();
        var allDrugs = await _drugRepository.GetByIdsAsync(distinctIds);
        if (allDrugs.Count != distinctIds.Length)
            throw new ArgumentException("One or more drug IDs not found in catalog");

        // Skip unverified drugs from the interaction check (they have no catalog match,
        // so we cannot reliably check what they interact with).
        var drugs = allDrugs.Where(d => d.IsVerified).ToList();
        if (drugs.Count == 0)
            throw new ArgumentException("No verified drugs to check - selected drugs are not in the catalog");

        // DUPLICATE DETECTION
        var duplicates = _duplicateDetector.Detect(drugs);

        // CACHE LOOKUP
        var cacheKey = _cacheHasher.Build(distinctIds, request.PatientId);
        var cached = await _interactionRepository.GetCachedByKeyAsync(
            cacheKey, TimeSpan.FromHours(1));
        if (cached != null)
            return MapToResponse(cached, drugs);

        // LOAD PATIENT CONTEXT
        Patient? patient = null;
        IReadOnlyList<PatientActiveMedication> currentMeds = Array.Empty<PatientActiveMedication>();
        if (request.PatientId.HasValue)
        {
            patient = await _patientRepository.GetByIdAsync(request.PatientId.Value);
            if (patient == null)
                throw new ArgumentException("Patient not found");

            if (!string.IsNullOrWhiteSpace(accountId) &&
                !string.Equals(patient.AccountId, accountId, StringComparison.Ordinal))
                throw new UnauthorizedAccessException("This patient does not belong to you");

            currentMeds = await _patientMeds.GetActiveMedicationsForPatientAsync(
                request.PatientId.Value, cancellationToken);
        }

        // HARD RULE - ALLERGY CHECK
        var allergyResult = AllergyMatchResult.NoMatch();
        if (patient != null)
        {
            var allergies = SplitSafely(patient.Allergies);
            allergyResult = _allergyMatcher.Check(drugs.Select(d => d.DrugName), allergies);
        }

        // HARD RULE - CRITICAL-PAIR CHECK
        var allIdsForPairCheck = distinctIds
            .Concat(currentMeds.Select(m => m.DrugId))
            .Distinct()
            .ToArray();
        var criticalPairs = await _criticalPairLookup.FindAllPairsAsync(allIdsForPairCheck);

        // Also check by scientific name (for "all NSAIDs + Warfarin" type rules)
        var allScientificNames = drugs.Select(d => d.DrugName)
            .Concat(currentMeds.Select(m => m.ScientificName ?? m.DrugName))
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToArray();
        var scientificCriticalPairs = await _criticalPairLookup
            .FindByScientificNamesAsync(allScientificNames);
        var allCriticalPairs = criticalPairs.Concat(scientificCriticalPairs).ToArray();

        // FAST-FAIL on hard rules
        LangflowInteractionResult verdict;
        if (allergyResult.HasMatch)
        {
            verdict = BuildAllergyVerdict(drugs, allergyResult);
        }
        else if (allCriticalPairs.Length > 0)
        {
            verdict = BuildCriticalPairVerdict(drugs, allCriticalPairs);
        }
        else
        {
            // CALL LANGFLOW
            var langflowRequest = BuildLangflowRequest(drugs, patient, currentMeds);
            LangflowInteractionResult? llmResult = null;
            try
            {
                llmResult = await _langflowClient.CheckMultiDrugInteractionAsync(
                    langflowRequest, cancellationToken);
            }
            catch
            {
                // swallow - fall back below
            }
            verdict = llmResult ?? BuildPrecautionaryVerdict(drugs);
        }

        // COMBINE SIGNALS via SeverityCalculator
        var finalLevel = _severityCalc.Calculate(new SeveritySignals(
            HasAllergyMatch: allergyResult.HasMatch,
            HasCriticalPairMatch: allCriticalPairs.Length > 0,
            HasDuplicateDrugs: duplicates.HasDuplicates,
            LlmDerivedLevel: verdict.Level
        ));
        // If severity calc upgraded the level, propagate it
        if (finalLevel != verdict.Level)
        {
            verdict = verdict with { Level = finalLevel };
        }

        // PERSIST
        var consentRecordId = await ResolveConsentRecordIdAsync(patient);
        var saved = await PersistAsync(
            request, distinctIds, drugs, cacheKey, verdict,
            modelVersion: verdict.ModelVersion,
            pineconeVersion: "safedose-drugs-v1",
            consentRecordId: consentRecordId,
            accountId: patient?.AccountId);

        // COMPLIANCE AUDIT
        if (patient != null)
        {
            await _audit.WriteAsync(new AuditLogEntry(
                AccountId: patient.AccountId,
                EntityName: nameof(InteractionCheck),
                EntityRowId: saved.InteractionCheckId,
                ActionType: 5,                              // 5 = DrugInteractionCheck
                AccessReason: $"Multi-drug check (level {(int)saved.SeverityLevel})"
            ), cancellationToken);
        }

        // RESPONSE
        return MapToResponse(saved, drugs);
    }

    private static LangflowInteractionResult BuildAllergyVerdict(
        IReadOnlyList<Drug> drugs,
        AllergyMatchResult allergy)
    {
        var analyzed = drugs.Select(d => new LangflowAnalyzedDrug(
            d.DrugId, d.DrugName, d.DrugName, d.Dose, "primary"
        )).ToArray();

        return new LangflowInteractionResult(
            Level: InteractionLevel.Danger,
            LabelArabic: "خطر",
            TitleArabic: "تنبيه: حساسية موثقة لدى المريض",
            ExplanationArabic: $"الدواء '{allergy.MatchedDrug}' قد يسبب رد فعل تحسسي بسبب الحساسية المسجلة لـ '{allergy.PatientAllergy}'. " + allergy.ReasonEnglish,
            RecommendedActionArabic: "تجنّب هذا الدواء واستشر طبيبك فوراً لاختيار بديل آمن",
            ConflictingPairs: Array.Empty<LangflowConflictingPair>(),
            AnalyzedDrugs: analyzed,
            Sources: new[] { "Patient Allergy Record", "SafeDose Cross-Reactivity Table" },
            ModelVersion: "hard-rule-allergy"
        );
    }

    private static LangflowInteractionResult BuildCriticalPairVerdict(
        IReadOnlyList<Drug> drugs,
        IReadOnlyList<CriticalPair> pairs)
    {
        var conflicting = pairs.Select(p => new LangflowConflictingPair(
            DrugA: p.DrugA?.DrugName ?? p.ScientificNameA ?? "?",
            DrugB: p.DrugB?.DrugName ?? p.ScientificNameB ?? "?",
            ReasonArabic: p.ReasonArabic,
            Severity: "high"
        )).ToArray();

        var analyzed = drugs.Select(d => new LangflowAnalyzedDrug(
            d.DrugId, d.DrugName, d.DrugName, d.Dose, "primary"
        )).ToArray();

        return new LangflowInteractionResult(
            Level: InteractionLevel.Danger,
            LabelArabic: "خطر",
            TitleArabic: "تفاعل دوائي ذو خطورة عالية يتطلب إشرافاً طبياً فوراً",
            ExplanationArabic: pairs.First().ReasonArabic,
            RecommendedActionArabic: "لا تجمع بين هذه الأدوية واستشر طبيبك فوراً",
            ConflictingPairs: conflicting,
            AnalyzedDrugs: analyzed,
            Sources: pairs.Select(p => p.Source).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToArray(),
            ModelVersion: "hard-rule-critical-pair"
        );
    }

    private static LangflowInteractionResult BuildPrecautionaryVerdict(IReadOnlyList<Drug> drugs)
    {
        var analyzed = drugs.Select(d => new LangflowAnalyzedDrug(
            d.DrugId, d.DrugName, d.DrugName, d.Dose, "noted"
        )).ToArray();

        return new LangflowInteractionResult(
            Level: InteractionLevel.Caution,
            LabelArabic: "احذر",
            TitleArabic: "تعذّر التحقق التلقائي من التفاعل",
            ExplanationArabic: "لم نتمكن من الوصول إلى نظام الفحص الذكي حالياً. لسلامتك، يُنصح بمراجعة طبيبك قبل تناول هذه الأدوية معاً.",
            RecommendedActionArabic: "راقب الأعراض، وإذا لاحظت أي تغير استشر طبيبك أو الصيدلي",
            ConflictingPairs: Array.Empty<LangflowConflictingPair>(),
            AnalyzedDrugs: analyzed,
            Sources: new[] { "Precautionary fallback" },
            ModelVersion: "fallback-precaution"
        );
    }

    private static LangflowInteractionRequest BuildLangflowRequest(
        IReadOnlyList<Drug> drugs,
        Patient? patient,
        IReadOnlyList<PatientActiveMedication> currentMeds)
    {
        var drugInputs = drugs.Select(d => new LangflowDrugInput(
            d.DrugId, d.DrugName, null, null
        )).ToArray();

        LangflowPatientContext? context = null;
        if (patient != null)
        {
            var age = patient.DateOfBirth.HasValue
                ? (int)((DateTime.UtcNow - patient.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25)
                : 0;

            context = new LangflowPatientContext(
                Age: age,
                Gender: patient.Gender?.ToString() ?? "unspecified",
                ChronicConditions: SplitSafely(patient.ChronicConditions),
                Allergies: SplitSafely(patient.Allergies),
                CurrentMedications: currentMeds.Select(m => new LangflowDrugInput(
                    m.DrugId, m.DrugName, m.ScientificName, null
                )).ToArray()
            );
        }

        return new LangflowInteractionRequest(drugInputs, context);
    }

    private async Task<int?> ResolveConsentRecordIdAsync(Patient? patient)
    {
        // TODO: query latest active consent record once IConsentRepository is available.
        await Task.CompletedTask;
        return null;
    }

    private async Task<InteractionCheck> PersistAsync(
        CheckInteractionsRequestDto request,
        int[] drugIds,
        IReadOnlyList<Drug> drugs,
        string cacheKey,
        LangflowInteractionResult verdict,
        string modelVersion,
        string pineconeVersion,
        int? consentRecordId,
        string? accountId)
    {
        var check = new InteractionCheck
        {
            PatientId = request.PatientId,
            TriggerType = request.TriggerType,
            DrugCount = (byte)drugIds.Length,
            CheckedDrugsJson = JsonSerializer.Serialize(
                drugs.Select(d => new { d.DrugId, d.DrugName, d.Dose })),
            SeverityLevel = verdict.Level,
            LabelArabic = verdict.LabelArabic,
            TitleArabic = verdict.TitleArabic,
            ExplanationArabic = verdict.ExplanationArabic,
            RecommendedActionArabic = verdict.RecommendedActionArabic,
            ConflictingPairsJson = JsonSerializer.Serialize(verdict.ConflictingPairs),
            SourcesJson = JsonSerializer.Serialize(verdict.Sources),
            ModelVersion = modelVersion,
            PineconeIndexVersion = pineconeVersion,
            CacheKey = cacheKey,
            ConsentRecordId = consentRecordId,
            AccountId = accountId,
            CheckedAt = DateTime.UtcNow,
        };

        await _interactionRepository.AddAsync(check);
        return check;
    }

    private static CheckInteractionsResponseDto MapToResponse(
        InteractionCheck check,
        IReadOnlyList<Drug> drugs)
    {
        var conflictingPairs = string.IsNullOrEmpty(check.ConflictingPairsJson)
            ? Array.Empty<ConflictingPairDto>()
            : JsonSerializer.Deserialize<ConflictingPairDto[]>(check.ConflictingPairsJson)
                ?? Array.Empty<ConflictingPairDto>();
        var sources = string.IsNullOrEmpty(check.SourcesJson)
            ? Array.Empty<string>()
            : JsonSerializer.Deserialize<string[]>(check.SourcesJson) ?? Array.Empty<string>();

        var analyzed = drugs.Select(d => new AnalyzedDrugDto(
            d.DrugId, d.DrugName, d.DrugName, d.Dose, "primary"
        )).ToArray();

        return new CheckInteractionsResponseDto(
            InteractionCheckId: check.InteractionCheckId,
            Level: check.SeverityLevel,
            LabelArabic: check.LabelArabic,
            Color: ColorForLevel(check.SeverityLevel),
            TitleArabic: check.TitleArabic,
            ExplanationArabic: check.ExplanationArabic,
            RecommendedActionArabic: check.RecommendedActionArabic,
            AnalyzedDrugs: analyzed,
            ConflictingPairs: conflictingPairs,
            Sources: sources,
            SafetyDisclaimerArabic: check.SafetyDisclaimerArabic,
            CheckedAt: check.CheckedAt
        );
    }

    private static string ColorForLevel(InteractionLevel level) => level switch
    {
        InteractionLevel.Safe => "#4CAF50",
        InteractionLevel.Caution => "#FFA000",
        InteractionLevel.Danger => "#D32F2F",
        _ => "#9E9E9E"
    };

    private static string[] SplitSafely(string? csv)
        => string.IsNullOrWhiteSpace(csv)
            ? Array.Empty<string>()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
