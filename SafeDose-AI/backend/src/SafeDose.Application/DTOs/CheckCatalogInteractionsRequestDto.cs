namespace SafeDose.Application.DTOs;

// On-demand check from the "افحص التداخلات الدوائية الآن" page.
// User picks drugs straight from the 22,574-row catalog (no save required).
// Optional PatientId pulls in the patient's verified active meds + allergies as context.
public record CheckCatalogInteractionsRequestDto(
    int[] DrugCatalogIds,
    int? PatientId = null
);
