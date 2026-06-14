using SafeDose.Application.DTOs;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Medication;

// Shared mapping + Arabic label resolvers - used by all medication use cases.
internal static class MedicationMappers
{
    public static MedicationResponseDto ToDto(PatientMedication pm)
    {
        var isVerified = pm.Drug?.IsVerified ?? false;
        return new MedicationResponseDto(
            PatientMedicationId: pm.PatientMedicationId,
            PatientId: pm.PatientId,
            DrugId: pm.DrugId,
            DrugName: pm.Drug?.DrugName ?? "(unknown)",
            DrugDose: pm.Drug?.Dose,
            PrescriptionId: pm.PrescriptionId,
            Dose: pm.Dose,
            Frequency: pm.Frequency,
            StartDate: pm.StartDate,
            EndDate: pm.EndDate,
            MealTiming: pm.MealTiming,
            Status: pm.Status,
            StatusArabic: StatusLabel(pm.Status),
            MealTimingArabic: MealTimingLabel(pm.MealTiming),
            IsVerified: isVerified,
            VerificationLabelArabic: isVerified ? "موثق" : "غير موثق",
            DrugCatalogId: pm.Drug?.DrugCatalogId
        );
    }

    public static string StatusLabel(byte status) => status switch
    {
        1 => "نشط",
        2 => "متوقف مؤقتاً",
        3 => "متوقف",
        _ => "غير معروف"
    };

    public static string? MealTimingLabel(byte? timing) => timing switch
    {
        1 => "قبل الأكل",
        2 => "مع الأكل",
        3 => "بعد الأكل",
        4 => "قبل النوم",
        _ => null
    };
}
