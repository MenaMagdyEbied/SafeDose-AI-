using SafeDose.Application.DTOs.PrescriptionDTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases;

public class SavePrescriptionUseCase
{
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IPatientRepository _patientRepository;

    public SavePrescriptionUseCase(
        IPrescriptionRepository prescriptionRepository,
        IPatientRepository patientRepository)
    {
        _prescriptionRepository = prescriptionRepository;
        _patientRepository = patientRepository;
    }

    public async Task<int> ExecuteAsync(SavePrescriptionDto dto, string accountId)
    {
        var patient = await _patientRepository.GetByIdAsync(dto.PatientId);
        if (patient == null)
        {
            throw new KeyNotFoundException("Patient not found.");
        }

        if (patient.AccountId != accountId)
        {
            throw new UnauthorizedAccessException("This patient does not belong to your account.");
        }

        var prescription = new Prescription
        {
            PatientId = dto.PatientId,
            PrescriptionName = dto.PrescriptionName,
            ImageUrl = dto.ImageUrl,
            AccountId = accountId,
            CreatedAt = DateTime.UtcNow,
            SourceType = 1,
            OCRStatus = 2
        };

        foreach (var drugDto in dto.Drugs)
        {
            var drug = new Drug
            {
                DrugName = drugDto.DrugName,
                Dose = drugDto.Dose,
                DoctorName = drugDto.DoctorName,
                Route = drugDto.Route,
                AccountId = accountId
            };

            var patientMedication = new PatientMedication
            {
                PatientId = dto.PatientId,
                Dose = drugDto.Dose,
                Frequency = drugDto.Frequency,
                StartDate = drugDto.StartDate,
                EndDate = drugDto.EndDate,
                MealTiming = drugDto.MealTiming,
                Status = 1,
                AccountId = accountId,
                Drug = drug
            };

            drug.PatientMedications.Add(patientMedication);
            prescription.Drugs?.Add(drug);
        }

        return await _prescriptionRepository.SavePrescriptionWithDrugsAsync(prescription);
    }
}
