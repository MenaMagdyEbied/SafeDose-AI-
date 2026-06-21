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
            // Add prescription (this will also add Drugs and PatientMedications attached to Drugs)
            await _db.Prescriptions.AddAsync(prescription);
            await _db.SaveChangesAsync();

            // Make sure the PatientMedication has the correct PrescriptionId populated
            if (prescription.Drugs != null)
            {
                foreach (var drug in prescription.Drugs)
                {
                    foreach (var patientMedication in drug.PatientMedications)
                    {
                        patientMedication.PrescriptionId = prescription.PrescriptionId;
                    }
                }
                // Save again to update the PrescriptionId in PatientMedications
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

    public async Task<List<Prescription>> GetPrescriptionsByPatientIdAsync(int patientId)
    {
        return await _db.Prescriptions
            .Where(p => p.PatientId == patientId)
            .Include(p => p.Drugs)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Prescription?> GetPrescriptionDetailsByIdAsync(int prescriptionId)
    {
        return await _db.Prescriptions
            .Where(p => p.PrescriptionId == prescriptionId)
            .Include(p => p.Drugs)
                .ThenInclude(d => d.PatientMedications)
            .FirstOrDefaultAsync();
    }
}
