namespace OnlineLearningPlatform.BusinessObject.Responses.Wallet
{
    public class WalletResponse
    {
        public Guid WalletId { get; set; }
        public decimal Balance { get; set; }
        public decimal PendingBalance { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalWithdrawn { get; set; }
        public int Status { get; set; }
        public List<WalletTransactionResponse> Transactions { get; set; } = new();
    }
}