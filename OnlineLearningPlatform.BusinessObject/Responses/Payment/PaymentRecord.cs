namespace OnlineLearningPlatform.BusinessObject.Responses.Payment
{
    public class PaymentRecord
    {
        public Guid PaymentId { get; set; }
        public long OrderCode { get; set; }
        public string? StudentName { get; set; }
        public decimal Amount { get; set; }
        public string? CourseTitle { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? UserEmail { get; set; }
    }
}
