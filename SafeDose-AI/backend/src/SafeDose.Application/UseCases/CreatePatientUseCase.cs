using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases;

// FR-201 — create a Patient linked to the authenticated Account.
public class CreatePatientUseCase
{
    private readonly IPatientRepository _patients;
    private readonly IAuditLogService _audit;

    public CreatePatientUseCase(IPatientRepository patients, IAuditLogService audit)
    {
        _patients = patients;
        _audit = audit;
    }

    public async Task<PatientResponseDto> ExecuteAsync(
        string accountId,
        CreatePatientDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("AccountId is required");

        var name = (dto.FullName ?? string.Empty).Trim();
        if (name.Length < 2 || name.Length > 150)
            throw new ArgumentException("FullName must be 2-150 characters");

        ValidateDob(dto.DateOfBirth);
        ValidateGender(dto.Gender);
        ValidateBloodType(dto.BloodType);

        var patient = new Patient
        {
            AccountId = accountId,
            FullName = name,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            BloodType = dto.BloodType,
            ChronicConditions = JoinTags(dto.ChronicConditions),
            Allergies = JoinTags(dto.Allergies),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        var newId = await _patients.CreateAsync(patient);

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: accountId,
            EntityName: nameof(Patient),
            EntityRowId: newId,
            ActionType: 2,                              // 2 = Create
            AccessReason: "Patient profile created"
        ), cancellationToken);

        return MapToResponse(patient);
    }

    private static void ValidateDob(DateOnly? dob)
    {
        if (!dob.HasValue) return;
        var min = new DateOnly(1900, 1, 1);
        var max = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dob.Value < min || dob.Value > max)
            throw new ArgumentException("DateOfBirth must be between 1900 and today");
    }

    private static void ValidateGender(byte? gender)
    {
        if (gender.HasValue && gender.Value != 1 && gender.Value != 2 && gender.Value != 3)
            throw new ArgumentException("Gender must be 1, 2, or 3");
    }

    private static readonly HashSet<string> AllowedBloodTypes = new(StringComparer.OrdinalIgnoreCase)
    { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

    private static void ValidateBloodType(string? bt)
    {
        if (string.IsNullOrEmpty(bt)) return;
        if (!AllowedBloodTypes.Contains(bt))
            throw new ArgumentException("BloodType must be one of: A+, A-, B+, B-, AB+, AB-, O+, O-");
    }

    private static string? JoinTags(string[]? tags)
        => tags is null || tags.Length == 0
            ? null
            : string.Join(",", tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim()));

    internal static PatientResponseDto MapToResponse(Patient p)
        => new(
            PatientId: p.PatientId,
            FullName: p.FullName,
            DateOfBirth: p.DateOfBirth,
            Gender: p.Gender,
            BloodType: p.BloodType,
            ChronicConditions: SplitTags(p.ChronicConditions),
            Allergies: SplitTags(p.Allergies),
            IsActive: p.IsActive,
            CreatedAt: p.CreatedAt
        );

    private static string[] SplitTags(string? csv)
        => string.IsNullOrWhiteSpace(csv)
            ? Array.Empty<string>()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
