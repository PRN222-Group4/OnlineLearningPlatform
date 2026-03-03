namespace OnlineLearningPlatform.BusinessObject.Requests.Course
{
    public class ApproveCourseRequest
    {
        public Guid CourseId { get; set; }
        public string? RejectReason { get; set; }
        public bool Status { get; set; }
    }
}
