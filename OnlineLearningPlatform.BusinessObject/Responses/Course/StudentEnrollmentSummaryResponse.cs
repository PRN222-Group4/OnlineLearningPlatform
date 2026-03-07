namespace OnlineLearningPlatform.BusinessObject.Responses.Course
{
    public class StudentEnrollmentSummaryResponse
    {
        public Guid EnrollmentId { get; set; }
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string? CourseImage { get; set; }
        public decimal ProgressPercent { get; set; }
        public DateTime? EnrolledAt { get; set; }
    }
}
