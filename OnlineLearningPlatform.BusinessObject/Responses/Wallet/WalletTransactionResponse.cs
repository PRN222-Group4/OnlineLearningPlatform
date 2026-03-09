namespace OnlineLearningPlatform.BusinessObject.Responses.Wallet
{
    public class WalletTransactionResponse
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public int TransactionType { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal BalanceAfterTransaction { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}