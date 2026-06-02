using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int patientId);
    Task<IEnumerable<Patient>> GetByAccountIdAsync(int accountId);
    Task<int> CreateAsync(Patient patient);
}
