using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SafeDose.Application.DTOs.PrescriptionDTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

public class GetPatientPrescriptionsUseCase
{
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IPatientRepository _patientRepository;

    public GetPatientPrescriptionsUseCase(
        IPrescriptionRepository prescriptionRepository,
        IPatientRepository patientRepository)
    {
        _prescriptionRepository = prescriptionRepository;
        _patientRepository = patientRepository;
    }

    public async Task<List<PrescriptionSummaryDto>> ExecuteAsync(int patientId, string accountId)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null)
            throw new KeyNotFoundException("Patient not found.");

        if (patient.AccountId != accountId)
            throw new UnauthorizedAccessException("This patient does not belong to your account.");

        var prescriptions = await _prescriptionRepository.GetPrescriptionsByPatientIdAsync(patientId);

        var result = new List<PrescriptionSummaryDto>();
        foreach (var p in prescriptions)
        {
            result.Add(new PrescriptionSummaryDto
            {
                PrescriptionId = p.PrescriptionId,
                PrescriptionName = p.PrescriptionName ?? "روشتة بدون اسم",
                Date = p.CreatedAt.ToString("dd/MM/yyyy"),
                DrugCount = p.Drugs?.Count ?? 0,
                DrugNames = p.Drugs?.Select(d => d.DrugName ?? string.Empty).ToList() ?? new List<string>()
            });
        }

        return result;
    }
}
