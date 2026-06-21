using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

public interface IPrescriptionRepository
{
    Task<int> SavePrescriptionWithDrugsAsync(Prescription prescription);
}
