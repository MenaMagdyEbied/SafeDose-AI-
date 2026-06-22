namespace SafeDose.Application.DTOs.Chatbot;

// PatientId is OPTIONAL now. If omitted on a family account (>1 patient), the
// bot returns intent="needs_patient_selection" with a list of patients so the
// frontend can show a picker. The user picks → frontend sends a new request
// with patientId set.
public record ChatRequestDto(
    string Message,
    int?   PatientId = null
);

public record PublicChatRequestDto(
    string Message
);

public record ChatPatientOptionDto(
    int    PatientId,
    string FullName,
    int?   Age,
    string? Gender
);

public record ChatResponseDto(
    string Reply,
    string Intent,
    int    PromptTokens,
    int    CompletionTokens,
    // Populated ONLY when Intent == "needs_patient_selection".
    // Frontend shows these as buttons/cards; user taps one, sends a new chat
    // request with the chosen PatientId.
    IReadOnlyList<ChatPatientOptionDto>? AvailablePatients = null
);
