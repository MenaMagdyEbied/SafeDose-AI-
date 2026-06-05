namespace SafeDose.Domain.Entities
{
    public class ConsentRecord
    {
        public int ConsentRecordId { get; set; }
        public string AccountId { get; set; }
        public string ConsentVersion { get; set; } = null!;
        public byte Consent_Type { get; set; }
        public bool IsGranted { get; set; }
        public DateTime GrantedAt { get; set; }

        public Account Account { get; set; } = null!;
    }
}
