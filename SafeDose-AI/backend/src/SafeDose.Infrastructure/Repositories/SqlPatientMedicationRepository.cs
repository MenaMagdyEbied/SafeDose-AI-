using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

// Replaces SqlPatientMedicationProvider.
// Implements BOTH IPatientMedicationRepository (Module 4 full CRUD)
// AND IPatientMedicationProvider (Module 5 read-only contract) —
// so a single DI registration serves both modules.
public class SqlPatientMedicationRepository
    : IPatientMedicationRepository, IPatientMedicationProvider
{
    private readonly AppDbContext _db;
    private const byte ActiveStatus = 1;

    public SqlPatientMedicationRepository(AppDbContext db)
    {
        _db = db;
    }

    // ─── IPatientMedicationProvider (Module 5) ──────────────────
    public async Task<IReadOnlyList<PatientActiveMedication>> GetActiveMedicationsForPatientAsync(
        int patientId, CancellationToken cancellationToken = default)
    {
        return await _db.PatientMedications
            .AsNoTracking()
            .Include(pm => pm.Drug)
            .Where(pm => pm.PatientId == patientId && pm.Status == ActiveStatus)
            .Select(pm => new PatientActiveMedication(
                pm.PatientMedicationId,
                pm.DrugId,
                pm.Drug.DrugName,
                pm.Dose,
                null  // ScientificName: enrich when Module 9 lands
            ))
            .ToListAsync(cancellationToken);
    }

    // ─── IPatientMedicationRepository (Module 4) ────────────────
    public Task<PatientMedication?> GetByIdAsync(int id)
        => _db.PatientMedications
            .Include(pm => pm.Drug)
            .FirstOrDefaultAsync(pm => pm.PatientMedicationId == id);

    public async Task<IReadOnlyList<PatientMedication>> GetAllForPatientAsync(int patientId)
    {
        return await _db.PatientMedications
            .AsNoTracking()
            .Include(pm => pm.Drug)
            .Where(pm => pm.PatientId == patientId)
            .OrderByDescending(pm => pm.StartDate ?? DateOnly.MinValue)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PatientMedication>> GetByStatusAsync(int patientId, byte status)
    {
        return await _db.PatientMedications
            .AsNoTracking()
            .Include(pm => pm.Drug)
            .Where(pm => pm.PatientId == patientId && pm.Status == status)
            .OrderByDescending(pm => pm.StartDate ?? DateOnly.MinValue)
            .ToListAsync();
    }

    public Task<bool> ExistsAsync(int id)
        => _db.PatientMedications.AnyAsync(pm => pm.PatientMedicationId == id);

    public async Task<int> CreateAsync(PatientMedication med)
    {
        await _db.PatientMedications.AddAsync(med);
        await _db.SaveChangesAsync();
        return med.PatientMedicationId;
    }

    public async Task CreateManyAsync(IEnumerable<PatientMedication> meds)
    {
        await _db.PatientMedications.AddRangeAsync(meds);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(PatientMedication med)
    {
        _db.PatientMedications.Update(med);
        await _db.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(int id, byte newStatus)
    {
        var med = await _db.PatientMedications.FindAsync(id);
        if (med == null) return;
        med.Status = newStatus;
        await _db.SaveChangesAsync();
    }

    public Task<bool> BelongsToAccountAsync(int id, string accountId)
        => _db.PatientMedications
            .AnyAsync(pm => pm.PatientMedicationId == id && pm.AccountId == accountId);

    public async Task<IReadOnlyList<PatientMedication>> GetExpiredActiveMedicationsAsync(DateOnly cutoff)
    {
        return await _db.PatientMedications
            .Where(pm => pm.Status == ActiveStatus
                      && pm.EndDate.HasValue
                      && pm.EndDate.Value < cutoff)
            .ToListAsync();
    }
}
