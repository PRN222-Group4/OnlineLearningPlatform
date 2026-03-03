namespace OnlineLearningPlatform.BusinessObject.Requests.Course
{
    public class GetAllCourseForAdminResponse
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public Guid CreatedBy { get; set; }
        public decimal Price { get; set; }
        public int Status { get; set; }
        public int Level { get; set; }
        public int ModuleCount { get; set; }
        public int LessonCount { get; set; }
        public int VideoCount { get; set; }
        public int ReadingCount { get; set; }
    }
}
