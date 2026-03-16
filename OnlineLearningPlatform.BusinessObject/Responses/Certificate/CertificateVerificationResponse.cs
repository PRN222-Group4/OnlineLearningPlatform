namespace OnlineLearningPlatform.BusinessObject.Responses.Certificate
{
    public class CertificateVerificationResponse
    {
        public string StudentName { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public DateTime IssueDate { get; set; }
        public string CertificateCode { get; set; } = null!;
        public string? CertificateUrl { get; set; }
    }
}