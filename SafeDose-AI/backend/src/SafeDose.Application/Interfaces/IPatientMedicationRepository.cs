using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

// Full CRUD for PatientMedication
// Extends the narrower IPatientMedicationProvider (read-only) used by Module 5.
// One concrete class implements BOTH so DI stays clean.
public interface IPatientMedicationRepository : IPatientMedicationProvider
{
    Task<PatientMedication?> GetByIdAsync(int patientMedicationId);
    Task<IReadOnlyList<PatientMedication>> GetAllForPatientAsync(int patientId);
    Task<IReadOnlyList<PatientMedication>> GetByStatusAsync(int patientId, byte status);
    Task<bool> ExistsAsync(int patientMedicationId);
    Task<int> CountActiveForPatientAsync(int patientId);

    Task<int> CreateAsync(PatientMedication med);
    Task CreateManyAsync(IEnumerable<PatientMedication> meds);
    Task UpdateAsync(PatientMedication med);
    Task ChangeStatusAsync(int id, byte newStatus);

    // Ownership check via denormalized AccountId on PatientMedication itself
    Task<bool> BelongsToAccountAsync(int patientMedicationId, string accountId);

    // Background job helper
    Task<IReadOnlyList<PatientMedication>> GetExpiredActiveMedicationsAsync(DateOnly cutoff);

    // Reminder schedule for a medication. The notification service reads these to fire pushes.
    // SetTimes wipes and replaces existing rows in PatientMedicationTime for the medication.
    Task SetTimesAsync(int patientMedicationId, string accountId, IEnumerable<TimeOnly> times);
    Task<IReadOnlyList<MedicationTimeView>> GetTimesAsync(int patientMedicationId);
}

// Read-only view of one configured reminder time.
public record MedicationTimeView(int PatientMedicationTimeId, TimeOnly Time);
