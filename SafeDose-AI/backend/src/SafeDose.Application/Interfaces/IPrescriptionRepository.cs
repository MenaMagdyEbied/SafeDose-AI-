using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

public interface IPrescriptionRepository
{
    Task<int> SavePrescriptionWithDrugsAsync(Prescription prescription);

    // Used by GetPatientPrescriptionsUseCase — list view, drugs included so we can show the count and names.
    Task<IReadOnlyList<Prescription>> GetPrescriptionsByPatientIdAsync(int patientId);

    // Used by GetPrescriptionDetailsUseCase — detail view, drugs + their patient-medication rows included.
    Task<Prescription?> GetPrescriptionDetailsByIdAsync(int prescriptionId);
}
