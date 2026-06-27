namespace SafeDose.Domain.Entities
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public string AccountId { get; set; }
        public string EntityName { get; set; } = null!;
        public int EntityRowId { get; set; }
        public byte ActionType { get; set; }
        public string? PHIAccessReason { get; set; }
        public DateTime CreatedAt { get; set; }

        public Account Account { get; set; } = null!;
    }

}
