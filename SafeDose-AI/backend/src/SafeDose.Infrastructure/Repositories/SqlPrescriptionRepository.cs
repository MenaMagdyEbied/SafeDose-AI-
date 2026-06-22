using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

public class SqlPrescriptionRepository : IPrescriptionRepository
{
    private readonly AppDbContext _db;

    public SqlPrescriptionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> SavePrescriptionWithDrugsAsync(Prescription prescription)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _db.Prescriptions.AddAsync(prescription);
            await _db.SaveChangesAsync();

            if (prescription.Drugs != null)
            {
                foreach (var drug in prescription.Drugs)
                {
                    foreach (var patientMedication in drug.PatientMedications)
                    {
                        patientMedication.PrescriptionId = prescription.PrescriptionId;
                    }
                }
                await _db.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return prescription.PrescriptionId;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<Prescription>> GetPrescriptionsByPatientIdAsync(int patientId)
    {
        return await _db.Prescriptions
            .AsNoTracking()
            .Where(p => p.PatientId == patientId)
            .Include(p => p.Drugs)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Prescription?> GetPrescriptionDetailsByIdAsync(int prescriptionId)
    {
        return await _db.Prescriptions
            .AsNoTracking()
            .Where(p => p.PrescriptionId == prescriptionId)
            .Include(p => p.Drugs!)
                .ThenInclude(d => d.PatientMedications)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeletePrescriptionAsync(int prescriptionId)
    {
        var prescription = await _db.Prescriptions
            .Include(p => p.Drugs!)
                .ThenInclude(d => d.PatientMedications)
            .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);
        if (prescription == null) return false;

        _db.Prescriptions.Remove(prescription);
        await _db.SaveChangesAsync();
        return true;
    }
}
