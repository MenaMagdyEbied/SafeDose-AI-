namespace SafeDose.Domain.Entities
{
    public class OTPRequest
    {
        public int OTPRequestId { get; set; }
        public string AccountId { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string HashedCode { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsVerified { get; set; }
        public int AttemptsCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public Account Account { get; set; } = null!;
    }
}
