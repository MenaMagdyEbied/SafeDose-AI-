using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

public class SqlPatientRepository : IPatientRepository
{
    private readonly AppDbContext _db;

    public SqlPatientRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Patient?> GetByIdAsync(int patientId)
        => _db.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId);

    public Task<Patient?> GetByIdIncludingInactiveAsync(int patientId)
        => _db.Patients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.PatientId == patientId);

    public async Task<IReadOnlyList<Patient>> GetByAccountIdAsync(
        string accountId, bool includeInactive = false)
    {
        var query = _db.Patients.AsNoTracking();
        if (includeInactive) query = query.IgnoreQueryFilters();

        return await query
            .Where(p => p.AccountId == accountId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public Task<int> CountByAccountIdAsync(string accountId)
        => _db.Patients.CountAsync(p => p.AccountId == accountId);

    public async Task<int> CreateAsync(Patient patient)
    {
        await _db.Patients.AddAsync(patient);
        await _db.SaveChangesAsync();
        return patient.PatientId;
    }

    public async Task UpdateAsync(Patient patient)
    {
        _db.Patients.Update(patient);
        await _db.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int patientId)
    {
        var patient = await _db.Patients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.PatientId == patientId);
        if (patient == null) return;

        patient.IsActive = false;
        patient.DeactivatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public Task<bool> ExistsForAccountAsync(int patientId, string accountId)
        => _db.Patients
            .IgnoreQueryFilters()
            .AnyAsync(p => p.PatientId == patientId && p.AccountId == accountId);
}
