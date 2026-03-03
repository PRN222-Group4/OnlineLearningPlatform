namespace OnlineLearningPlatform.BusinessObject.Requests.Course
{
    public class UpdateCourseRequest
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}
