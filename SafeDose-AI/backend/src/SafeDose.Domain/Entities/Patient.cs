namespace SafeDose.Domain.Entities;

public class Patient
{
    public int PatientId { get; set; }
    public int AccountId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public byte Gender { get; set; }
    public string? BloodType { get; set; }
    public string? ChronicConditions { get; set; }
    public string? Allergies { get; set; }
    public int? PrimaryDoctorId { get; set; }
    public DateTime CreatedAt { get; set; }
}
