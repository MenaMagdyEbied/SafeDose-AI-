namespace SafeDose.Application.DTOs.Chatbot;

// Primary patient (the one explicitly addressed in the chat request).
public record ChatbotPatientContextDto(
    int      PatientId,
    string   FullName,
    int?     Age,
    string?  Gender,
    string?  BloodType,
    string[] ChronicConditions,
    string[] Allergies,
    IReadOnlyList<ChatbotMedicationDto>     ActiveMedications,
    IReadOnlyList<ChatbotFamilyMemberDto>   OtherFamilyMembers   // everyone else on the same account
);

public record ChatbotMedicationDto(
    string  Name,
    string? ScientificName,
    string? Dose
);

// Lighter view per family member — bot uses this to switch context when the user
// asks about a different family member by name (e.g. "Kyrillos has fever").
public record ChatbotFamilyMemberDto(
    int      PatientId,
    string   FullName,
    int?     Age,
    string?  Gender,
    string[] ChronicConditions,
    string[] Allergies,
    IReadOnlyList<ChatbotMedicationDto> ActiveMedications
);
