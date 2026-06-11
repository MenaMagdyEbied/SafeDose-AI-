using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases;

// FR-204 — update editable fields of an owned Patient.
public class UpdatePatientUseCase
{
    private readonly IPatientRepository _patients;
    private readonly IAuditLogService _audit;

    public UpdatePatientUseCase(IPatientRepository patients, IAuditLogService audit)
    {
        _patients = patients;
        _audit = audit;
    }

    public async Task<PatientResponseDto?> ExecuteAsync(
        int patientId,
        string accountId,
        UpdatePatientDto dto,
        CancellationToken cancellationToken = default)
    {
        var patient = await _patients.GetByIdAsync(patientId);
        if (patient == null) return null;

        // Ownership check (FR-208)
        if (!string.Equals(patient.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("This patient does not belong to you");

        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            var name = dto.FullName.Trim();
            if (name.Length < 2 || name.Length > 150)
                throw new ArgumentException("FullName must be 2-150 characters");
            patient.FullName = name;
        }

        if (dto.DateOfBirth.HasValue)
        {
            var min = new DateOnly(1900, 1, 1);
            var max = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dto.DateOfBirth.Value < min || dto.DateOfBirth.Value > max)
                throw new ArgumentException("DateOfBirth must be between 1900 and today");
            patient.DateOfBirth = dto.DateOfBirth;
        }

        if (dto.Gender.HasValue)
        {
            if (dto.Gender.Value != 1 && dto.Gender.Value != 2 && dto.Gender.Value != 3)
                throw new ArgumentException("Gender must be 1, 2, or 3");
            patient.Gender = dto.Gender;
        }

        if (dto.BloodType != null)
        {
            patient.BloodType = string.IsNullOrEmpty(dto.BloodType) ? null : dto.BloodType;
        }

        if (dto.ChronicConditions != null)
        {
            patient.ChronicConditions = JoinTags(dto.ChronicConditions);
        }

        if (dto.Allergies != null)
        {
            patient.Allergies = JoinTags(dto.Allergies);
        }

        await _patients.UpdateAsync(patient);

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: accountId,
            EntityName: nameof(Patient),
            EntityRowId: patientId,
            ActionType: 3,                              // 3 = Update
            AccessReason: "Patient profile updated"
        ), cancellationToken);

        return CreatePatientUseCase.MapToResponse(patient);
    }

    private static string? JoinTags(string[]? tags)
        => tags is null || tags.Length == 0
            ? null
            : string.Join(",", tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim()));
}
