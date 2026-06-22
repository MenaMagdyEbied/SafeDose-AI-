using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

public class DeletePrescriptionUseCase
{
    private readonly IPrescriptionRepository _prescriptions;
    private readonly IPatientRepository _patients;

    public DeletePrescriptionUseCase(
        IPrescriptionRepository prescriptions,
        IPatientRepository patients)
    {
        _prescriptions = prescriptions;
        _patients = patients;
    }

    // Returns false if the prescription doesn't exist.
    // Throws UnauthorizedAccessException if it belongs to another account.
    public async Task<bool> ExecuteAsync(int prescriptionId, string accountId)
    {
        var prescription = await _prescriptions.GetPrescriptionDetailsByIdAsync(prescriptionId);
        if (prescription == null) return false;

        var patient = await _patients.GetByIdAsync(prescription.PatientId);
        if (patient == null || patient.AccountId != accountId)
            throw new UnauthorizedAccessException("You do not have access to this prescription.");

        return await _prescriptions.DeletePrescriptionAsync(prescriptionId);
    }
}
