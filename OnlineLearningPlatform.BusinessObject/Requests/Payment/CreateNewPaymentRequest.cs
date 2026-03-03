namespace OnlineLearningPlatform.BusinessObject.Requests.Payment
{
    public class CreateNewPaymentRequest
    {
        public Guid CourseId { get; set; }
        public decimal Amount { get; set; }
    }
}
