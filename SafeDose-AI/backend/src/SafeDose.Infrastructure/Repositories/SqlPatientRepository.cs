using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

public class SqlPatientRepository : IPatientRepository
{
    // TODO: inject DbContext via constructor when EF Core is added
    public Task<Patient?> GetByIdAsync(int patientId)
    {
        throw new NotImplementedException("");
    }

    public Task<IEnumerable<Patient>> GetByAccountIdAsync(int accountId)
    {
        throw new NotImplementedException("");
    }

    public Task<int> CreateAsync(Patient patient)
    {
        throw new NotImplementedException("");
    }
}