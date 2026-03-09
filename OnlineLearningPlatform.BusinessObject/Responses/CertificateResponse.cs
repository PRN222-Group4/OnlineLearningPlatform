namespace OnlineLearningPlatform.BusinessObject.Responses
{
    public class CertificateResponse
    {
        public Guid CertificateId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public string CourseImage { get; set; } = null!;
        public string InstructorName { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        public DateTime IssueDate { get; set; }
        public string CertificateCode { get; set; } = null!;
    }
}