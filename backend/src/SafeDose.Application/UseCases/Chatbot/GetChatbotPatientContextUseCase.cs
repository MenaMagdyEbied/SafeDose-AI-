using SafeDose.Application.DTOs.Chatbot;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases.Chatbot;

public class GetChatbotPatientContextUseCase
{
    private readonly IPatientRepository           _patients;
    private readonly IPatientMedicationProvider   _medications;

    public GetChatbotPatientContextUseCase(IPatientRepository patients, IPatientMedicationProvider medications)
    {
        _patients    = patients;
        _medications = medications;
    }

    public async Task<ChatbotPatientContextDto?> ExecuteAsync(
        string accountId,
        int? patientId = null,
        CancellationToken ct = default)
    {
        var allPatients = await _patients.GetByAccountIdAsync(accountId);
        if (allPatients.Count == 0) return null;

        var primary = patientId.HasValue
            ? allPatients.FirstOrDefault(p => p.PatientId == patientId.Value)
            : allPatients.First();
        if (primary == null) return null;

        return await BuildDtoAsync(primary, allPatients.Where(p => p.PatientId != primary.PatientId).ToList(), ct);
    }

    public async Task<ChatbotPatientContextDto?> ExecuteForServiceAsync(int patientId, CancellationToken ct = default)
    {
        var primary = await _patients.GetByIdAsync(patientId);
        if (primary == null) return null;
        var family = (await _patients.GetByAccountIdAsync(primary.AccountId))
            .Where(p => p.PatientId != primary.PatientId).ToList();
        return await BuildDtoAsync(primary, family, ct);
    }

    private async Task<ChatbotPatientContextDto> BuildDtoAsync(
        SafeDose.Domain.Entities.Patient primary,
        IReadOnlyList<SafeDose.Domain.Entities.Patient> family,
        CancellationToken ct)
    {
        var primaryMeds = await LoadMedsAsync(primary.PatientId, ct);

        var familyDtos = new List<ChatbotFamilyMemberDto>(family.Count);
        foreach (var p in family)
        {
            var meds = await LoadMedsAsync(p.PatientId, ct);
            familyDtos.Add(new ChatbotFamilyMemberDto(
                PatientId:         p.PatientId,
                FullName:          p.FullName,
                Age:               CalcAge(p.DateOfBirth),
                Gender:            MapGender(p.Gender),
                ChronicConditions: SplitCsv(p.ChronicConditions),
                Allergies:         SplitCsv(p.Allergies),
                ActiveMedications: meds));
        }

        return new ChatbotPatientContextDto(
            PatientId:          primary.PatientId,
            FullName:           primary.FullName,
            Age:                CalcAge(primary.DateOfBirth),
            Gender:             MapGender(primary.Gender),
            BloodType:          primary.BloodType,
            ChronicConditions:  SplitCsv(primary.ChronicConditions),
            Allergies:          SplitCsv(primary.Allergies),
            ActiveMedications:  primaryMeds,
            OtherFamilyMembers: familyDtos);
    }

    private async Task<IReadOnlyList<ChatbotMedicationDto>> LoadMedsAsync(int patientId, CancellationToken ct)
    {
        var meds = await _medications.GetActiveMedicationsForPatientAsync(patientId, ct);
        return meds.Select(m => new ChatbotMedicationDto(m.DrugName, m.ScientificName, m.Dose)).ToList();
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

    private static string[] SplitCsv(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return Array.Empty<string>();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
