using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SafeDose.Application.DTOs.PrescriptionDTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

public class GetPrescriptionDetailsUseCase
{
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IPatientRepository _patientRepository;

    public GetPrescriptionDetailsUseCase(
        IPrescriptionRepository prescriptionRepository,
        IPatientRepository patientRepository)
    {
        _prescriptionRepository = prescriptionRepository;
        _patientRepository = patientRepository;
    }

    public async Task<PrescriptionDetailDto> ExecuteAsync(int prescriptionId, string accountId)
    {
        var prescription = await _prescriptionRepository.GetPrescriptionDetailsByIdAsync(prescriptionId);
        if (prescription == null)
            throw new KeyNotFoundException("Prescription not found.");

        var patient = await _patientRepository.GetByIdAsync(prescription.PatientId);
        if (patient == null || patient.AccountId != accountId)
            throw new UnauthorizedAccessException("You do not have access to this prescription.");

        var dto = new PrescriptionDetailDto
        {
            PrescriptionId = prescription.PrescriptionId,
            PrescriptionName = prescription.PrescriptionName ?? "روشتة بدون اسم",
            Date = prescription.CreatedAt.ToString("dd/MM/yyyy"),
            DrugCount = prescription.Drugs?.Count ?? 0,
            Medications = new List<PrescriptionMedicationDto>()
        };

        if (prescription.Drugs != null)
        {
            foreach (var drug in prescription.Drugs)
            {
                var patientMed = drug.PatientMedications?.FirstOrDefault();
                
                string frequencyStr = "";
                string durationStr = "";
                
                if (patientMed != null)
                {
                    frequencyStr = patientMed.Frequency > 0 ? $"{patientMed.Frequency} مرات يومياً" : "";
                    
                    if (patientMed.StartDate.HasValue && patientMed.EndDate.HasValue)
                    {
                        var diff = patientMed.EndDate.Value.DayNumber - patientMed.StartDate.Value.DayNumber;
                        durationStr = diff > 0 ? $"لمدة {diff} أيام" : "مستمر";
                    }
                }

                dto.Medications.Add(new PrescriptionMedicationDto
                {
                    DrugName = drug.DrugName ?? "",
                    Dose = patientMed?.Dose ?? drug.Dose ?? "",
                    Frequency = frequencyStr,
                    Duration = durationStr
                });
            }
        }

        return dto;
    }
}
