using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

public interface IPrescriptionRepository
{
    Task<int> SavePrescriptionWithDrugsAsync(Prescription prescription);
    Task<List<Prescription>> GetPrescriptionsByPatientIdAsync(int patientId);
    Task<Prescription?> GetPrescriptionDetailsByIdAsync(int prescriptionId);
}
