using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

public class SqlPaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;

    public SqlPaymentRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Payment?> GetByIdAsync(int paymentId)
        => _db.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);

    public async Task<int> CreateAsync(Payment payment)
    {
        await _db.Payments.AddAsync(payment);
        await _db.SaveChangesAsync();
        return payment.PaymentId;
    }

    public async Task UpdateAsync(Payment payment)
    {
        _db.Payments.Update(payment);
        await _db.SaveChangesAsync();
    }

    public Task<Payment?> GetByGatewayReferenceAsync(string gateway, string gatewayReference)
        => _db.Payments
            .FirstOrDefaultAsync(p => p.GateWay == gateway
                                   && p.GateWayReference == gatewayReference);

    public Task<Payment?> GetByMerchantOrderIdAsync(string merchantOrderId)
    {
        if (string.IsNullOrWhiteSpace(merchantOrderId))
        {
            return Task.FromResult<Payment?>(null);
        }

        var candidate = merchantOrderId.Trim();
        if (candidate.StartsWith("SD-", StringComparison.OrdinalIgnoreCase))
            candidate = candidate[3..];

        if (!int.TryParse(candidate, out var paymentId))
            return Task.FromResult<Payment?>(null);

        return GetByIdAsync(paymentId);
    }
}
