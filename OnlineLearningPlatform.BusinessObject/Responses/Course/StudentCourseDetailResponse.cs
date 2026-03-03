namespace OnlineLearningPlatform.BusinessObject.Responses.Course
{
    public class StudentCourseDetailResponse
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
        public string Language { get; set; }
        public List<StudentModuleResponse> Modules { get; set; } = new List<StudentModuleResponse>();
    }
    public class StudentModuleResponse
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public int Index { get; set; }
        public List<StudentLessonResponse> Lessons { get; set; } = new List<StudentLessonResponse>();
    }

    public class StudentLessonResponse
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Duration { get; set; }
        public int OrderIndex { get; set; }
        public List<StudentLessonItemResponse> LessonItem { get; set; } = new List<StudentLessonItemResponse>();
    }

    public class StudentLessonItemResponse
    {
        public int Type { get; set; }
        public int OrderIndex { get; set; }
    }
}