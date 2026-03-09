namespace OnlineLearningPlatform.BusinessObject.Responses.Wallet
{
    public class PendingPayoutResponse
    {
        public Guid WalletId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string InstructorEmail { get; set; } = string.Empty;
        public decimal PendingBalance { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalWithdrawn { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}