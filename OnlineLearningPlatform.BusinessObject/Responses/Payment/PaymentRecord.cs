namespace OnlineLearningPlatform.BusinessObject.Responses.Payment
{
    public class PaymentRecord
    {
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? UserEmail { get; set; }
    }
}