namespace OnlineLearningPlatform.BusinessObject.Responses.Admin
{
    public class TopCourseResponse
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int EnrollCount { get; set; }
    }
}