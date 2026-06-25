using Microsoft.EntityFrameworkCore;
using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;

namespace SafeDose.Application.UseCases
{
    public class GetPrivateMedicalCardUseCase
    {
        private readonly AppDbContext _db;

        public GetPrivateMedicalCardUseCase(AppDbContext db)
        {
            _db = db;
        }

        public async Task<MedicalCardDto> ExecuteAsync(int patientId, string accountId)
        {
            var patient = await _db.Patients
                .Include(p => p.PatientMedications!)
                .ThenInclude(pm => pm.Drug)
                .FirstOrDefaultAsync(p => p.PatientId == patientId && p.AccountId == accountId);

            if (patient == null || !patient.IsActive)
            {
                throw new KeyNotFoundException("Patient not found or inactive.");
            }

            
            var currentMedications = patient.PatientMedications?
                .Where(pm => pm.Status == 1)
                .Select(pm => new MedicalCardDrugDto
                {
                    DrugName = pm.Drug?.DrugName,
                    Dose = pm.Dose,
                    Frequency = pm.Frequency
                })
                .ToList() ?? new List<MedicalCardDrugDto>();

            // Doctor name comes from the first active drug that has one set.
            // Patient itself doesn't carry a primary-doctor field.
            var doctorName = patient.PatientMedications?
                .Where(pm => pm.Status == 1 && !string.IsNullOrWhiteSpace(pm.Drug?.DoctorName))
                .Select(pm => pm.Drug!.DoctorName)
                .FirstOrDefault();

            return new MedicalCardDto
            {
                FullName = patient.FullName,
                BloodType = patient.BloodType,
                ChronicConditions = patient.ChronicConditions,
                Allergies = patient.Allergies,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                DoctorName = doctorName,
                CurrentMedications = currentMedications,
                MedicalCardToken = patient.MedicalCardToken
            };
        }
    }
}
