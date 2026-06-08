using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

// Reads PatientMedication directly from SQL.
// When Ahmed (Module 4) ships his repository, we can swap this implementation
// or keep this one — both layers will agree on the entity shape.
public class SqlPatientMedicationProvider : IPatientMedicationProvider
{
    private readonly AppDbContext _db;

    public SqlPatientMedicationProvider(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PatientActiveMedication>> GetActiveMedicationsForPatientAsync(
        int patientId,
        CancellationToken cancellationToken = default)
    {
        // Status convention: 1 = active. Module 4 should standardize this.
        const byte ActiveStatus = 1;

        var meds = await _db.PatientMedications
            .AsNoTracking()
            .Include(pm => pm.Drug)
            .Where(pm => pm.PatientId == patientId && pm.Status == ActiveStatus)
            .Select(pm => new PatientActiveMedication(
                pm.PatientMedicationId,
                pm.DrugId,
                pm.Drug.DrugName,
                pm.Dose,
                null  // ScientificName: enrich when Module 8 lands
            ))
            .ToListAsync(cancellationToken);

        return meds;
    }
}
