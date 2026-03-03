namespace OnlineLearningPlatform.BusinessObject.Responses.Course
{
    public class CourseDetailResponse
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public string Level { get; set; }
        public string Category { get; set; }
        public double Rating { get; set; }
        public int Students { get; set; }
        public string Duration { get; set; }
        public string? InstructorName { get; set; }

        public List<PublicCourseModule> Modules { get; set; } = new List<PublicCourseModule>();
    }

    public class PublicCourseModule
    {
        public string Title { get; set; }
        public List<PublicCourseLesson> Lessons { get; set; } = new List<PublicCourseLesson>();
    }

    public class PublicCourseLesson
    {
        public string Title { get; set; }
        public string Duration { get; set; }
        public string Type { get; set; }
    }
}