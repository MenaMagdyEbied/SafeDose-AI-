namespace SafeDose.Application.DTOs;

// What the UI sends when the user clicks "افحص التداخلات الدوائية الآن"
// Validation rules:
//  - 1..6 drugs (UI cap)
//  - All DrugIds must exist in our catalog
//  - PatientId optional - anonymous check allowed
public record CheckInteractionsRequestDto(
    int[] DrugIds,
    int? PatientId = null,
    byte TriggerType = 1   // 1=Manual, 2=Prescription, 3=Barcode, 4=Voice
);
