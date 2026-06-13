using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

public class SqlAuditLogService : IAuditLogService
{
    private readonly AppDbContext _db;

    public SqlAuditLogService(AppDbContext db)
    {
        _db = db;
    }

    public async Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entry.AccountId)) return; // anonymous actions skip audit

        var log = new AuditLog
        {
            AccountId = entry.AccountId,
            EntityName = entry.EntityName,
            EntityRowId = entry.EntityRowId,
            ActionType = entry.ActionType,
            PHIAccessReason = entry.AccessReason,
            CreatedAt = DateTime.UtcNow,
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
