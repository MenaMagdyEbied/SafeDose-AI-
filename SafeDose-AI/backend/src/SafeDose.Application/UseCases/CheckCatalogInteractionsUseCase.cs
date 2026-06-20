using System.Text.Json;
using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.UseCases;

public class CheckCatalogInteractionsUseCase
{
    private readonly IDrugRepository _drugs;
    private readonly IPatientRepository _patients;
    private readonly IPatientMedicationProvider _patientMeds;
    private readonly ILangflowClient _langflow;
    private readonly IInteractionRepository _interactions;
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IPricingTierRepository _tiers;

    public CheckCatalogInteractionsUseCase(
        IDrugRepository drugs,
        IPatientRepository patients,
        IPatientMedicationProvider patientMeds,
        ILangflowClient langflow,
        IInteractionRepository interactions,
        ISubscriptionRepository subscriptions,
        IPricingTierRepository tiers)
    {
        _drugs = drugs;
        _patients = patients;
        _patientMeds = patientMeds;
        _langflow = langflow;
        _interactions = interactions;
        _subscriptions = subscriptions;
        _tiers = tiers;
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

        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("AccountId required");

        var tier = await ResolveTierAsync(accountId);
        await EnforceDailyInteractionLimitAsync(accountId, tier);

        var catalogEntries = new List<LangflowDrugInput>();
        var nameLookup = new Dictionary<int, (string En, string? Ar)>();
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
            nameLookup[entry.DrugCatalogId] = (entry.CommercialNameEn, entry.CommercialNameAr);
        }

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

        var result = await _langflow.CheckMultiDrugInteractionAsync(
            new LangflowInteractionRequest(catalogEntries.ToArray(), patientContext),
            cancellationToken);

        if (result == null)
        {
            var fallback = new CheckInteractionsResponseDto(
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
            var savedFallbackId = await PersistCheckAsync(
                request, catalogEntries, fallback, accountId, modelVersion: null, cancellationToken);
            return fallback with { InteractionCheckId = savedFallbackId };
        }

        var analyzedFromLangflow = result.AnalyzedDrugs?.ToDictionary(d => d.DrugId) ?? new();
        var analyzedDrugs = catalogEntries.Select(c =>
        {
            analyzedFromLangflow.TryGetValue(c.DrugId, out var fromLf);
            var (en, ar) = nameLookup[c.DrugId];
            return new AnalyzedDrugDto(
                DrugId: c.DrugId,
                ArabicName: !string.IsNullOrWhiteSpace(fromLf?.ArabicName) ? fromLf.ArabicName : ar ?? en,
                EnglishName: !string.IsNullOrWhiteSpace(fromLf?.EnglishName) ? fromLf.EnglishName : en,
                DosageNote: fromLf?.DosageNote,
                Role: !string.IsNullOrWhiteSpace(fromLf?.Role) ? fromLf.Role : "primary"
            );
        }).ToArray();

        var response = new CheckInteractionsResponseDto(
            InteractionCheckId: 0,
            Level: result.Level,
            LabelArabic: result.LabelArabic,
            Color: ColorFor(result.Level),
            TitleArabic: result.TitleArabic,
            ExplanationArabic: result.ExplanationArabic,
            RecommendedActionArabic: result.RecommendedActionArabic,
            AnalyzedDrugs: analyzedDrugs,
            ConflictingPairs: result.ConflictingPairs
                .Select(p => new ConflictingPairDto(p.DrugA, p.DrugB, p.ReasonArabic, p.Severity))
                .ToArray(),
            Sources: result.Sources,
            SafetyDisclaimerArabic: "استشر طبيبك أو الصيدلي",
            CheckedAt: DateTime.UtcNow
        );

        var savedId = await PersistCheckAsync(
            request, catalogEntries, response, accountId, modelVersion: result.ModelVersion, cancellationToken);
        return response with { InteractionCheckId = savedId };
    }

    private async Task<int> PersistCheckAsync(
        CheckCatalogInteractionsRequestDto request,
        List<LangflowDrugInput> drugs,
        CheckInteractionsResponseDto response,
        string? accountId,
        string? modelVersion,
        CancellationToken cancellationToken)
    {
        var record = new InteractionCheck
        {
            PatientId = request.PatientId,
            TriggerType = 1,
            DrugCount = (byte)drugs.Count,
            CheckedDrugsJson = JsonSerializer.Serialize(drugs.Select(d => new
            {
                d.DrugId,
                d.DrugName,
                d.ScientificName,
                d.DrugClass
            })),
            SeverityLevel = response.Level,
            LabelArabic = response.LabelArabic,
            TitleArabic = response.TitleArabic,
            ExplanationArabic = response.ExplanationArabic,
            RecommendedActionArabic = response.RecommendedActionArabic,
            ConflictingPairsJson = response.ConflictingPairs.Length > 0
                ? JsonSerializer.Serialize(response.ConflictingPairs) : null,
            SourcesJson = response.Sources.Length > 0
                ? JsonSerializer.Serialize(response.Sources) : null,
            SafetyDisclaimerArabic = response.SafetyDisclaimerArabic,
            ModelVersion = modelVersion,
            CheckedAt = response.CheckedAt,
            AccountId = accountId,
        };
        await _interactions.AddAsync(record);
        return record.InteractionCheckId;
    }

    private async Task<PricingTier> ResolveTierAsync(string accountId)
    {
        var subscription = await _subscriptions.GetActiveByAccountAsync(accountId);
        if (subscription?.PricingTier != null)
            return subscription.PricingTier;

        return await _tiers.GetByCodeAsync("free")
            ?? throw new InvalidOperationException("Free pricing tier is not configured");
    }

    private async Task EnforceDailyInteractionLimitAsync(string accountId, PricingTier tier)
    {
        if (tier.InteractionCheckLimitPerDay == int.MaxValue)
            return;

        var startOfTodayUtc = StartOfCairoDayAsUtc(DateTime.UtcNow);
        var usedToday = await _interactions.CountForAccountSinceAsync(accountId, startOfTodayUtc);
        if (usedToday >= tier.InteractionCheckLimitPerDay)
            throw new Exceptions.QuotaExceededException(
                $"وصلت إلى الحد الأقصى للفحوصات اليومية ({tier.InteractionCheckLimitPerDay} يومياً). اشترك في الباقة المدفوعة للحصول على عدد غير محدود.",
                $"Daily interaction check limit reached for your plan ({tier.InteractionCheckLimitPerDay} per day).");
    }

    private static DateTime StartOfCairoDayAsUtc(DateTime nowUtc)
    {
        var cairoTz = TryGetCairoTz();
        var cairoNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, cairoTz);
        var cairoMidnight = new DateTime(cairoNow.Year, cairoNow.Month, cairoNow.Day, 0, 0, 0, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(cairoMidnight, cairoTz);
    }

    private static TimeZoneInfo TryGetCairoTz()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo"); }
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
