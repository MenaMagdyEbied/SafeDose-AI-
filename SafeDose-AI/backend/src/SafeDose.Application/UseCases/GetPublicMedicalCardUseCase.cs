using Microsoft.EntityFrameworkCore;
using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;

namespace SafeDose.Application.UseCases
{
    public class GetPublicMedicalCardUseCase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly AppDbContext _db;

        public GetPublicMedicalCardUseCase(IPatientRepository patientRepository, AppDbContext db)
        {
            _patientRepository = patientRepository;
            _db = db;
        }

        public async Task<MedicalCardDto> ExecuteAsync(Guid token)
        {
           
            var patient = await _patientRepository.GetByMedicalCardTokenAsync(token);
            if (patient == null || !patient.IsActive)
            {
                throw new KeyNotFoundException("Medical card not found or inactive.");
            }

            // 2. Check subscription status (assuming Status 1 = Active, or EndAt > Now)
            // You might need to adjust the logic based on your exact Subscription Status enum
            var hasActiveSubscription = await _db.Subscriptions
                .Where(s => s.AccountId == patient.AccountId)
                .AnyAsync(s => s.Status == 2 && (s.EndAt == null || s.EndAt > DateTime.UtcNow));

            if (!hasActiveSubscription)
            {
                throw new UnauthorizedAccessException("Medical card is inactive due to expired subscription.");
            }

            var currentMedications = patient.PatientMedications?
                .Where(pm => pm.Status == 1) // Assuming 1 = Active
                .Select(pm => new MedicalCardDrugDto
                {
                    DrugName = pm.Drug?.DrugName,
                    Dose = pm.Dose,
                    Frequency = pm.Frequency
                })
                .ToList() ?? new List<MedicalCardDrugDto>();

            return new MedicalCardDto
            {
                FullName = patient.FullName,
                BloodType = patient.BloodType,
                ChronicConditions = patient.ChronicConditions,
                Allergies = patient.Allergies,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                CurrentMedications = currentMedications,
                MedicalCardToken = patient.MedicalCardToken
            };
        }
    }
}
