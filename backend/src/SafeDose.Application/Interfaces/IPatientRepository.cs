using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int patientId);
    Task<Patient?> GetByIdIncludingInactiveAsync(int patientId);
    Task<IReadOnlyList<Patient>> GetByAccountIdAsync(string accountId, bool includeInactive = false);
    Task<int> CountByAccountIdAsync(string accountId);
    Task<int> CreateAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task SoftDeleteAsync(int patientId);
    Task<bool> ExistsForAccountAsync(int patientId, string accountId);
    Task<Patient?> GetByMedicalCardTokenAsync(Guid token);
}
