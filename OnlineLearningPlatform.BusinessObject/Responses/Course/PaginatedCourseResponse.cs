using OnlineLearningPlatform.BusinessObject.Responses.Course;

namespace OnlineLearningPlatform.BusinessObject.Responses
{
    public class PaginatedCourseResponse
    {
        public List<CourseResponse> Courses { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}