using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

public interface IPaymentRepository
{
    Task<int> CreateAsync(Payment payment);
    Task UpdateAsync(Payment payment);

    // Used by the webhook to spot duplicates. If a payment with this gateway reference
    // already exists, we ignore the second webhook (idempotent).
    Task<Payment?> GetByGatewayReferenceAsync(string gateway, string gatewayReference);
}
