namespace SafeDose.Application.Interfaces;

// Required by Egyptian Data Protection Law 151/2020.
// Every action that touches Patient Health Information (PHI) must be logged
// with WHO did it, WHAT they did, and WHY they accessed it.
public interface IAuditLogService
{
    Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}

// Action codes (byte for compactness in SQL):
//   1 = Read
//   2 = Write/Create
//   3 = Update
//   4 = Delete
//   5 = DrugInteractionCheck
//   6 = PrescriptionUpload
//   7 = ChatMessage
public record AuditLogEntry(
    string AccountId,
    string EntityName,
    int EntityRowId,
    byte ActionType,
    string? AccessReason
);
