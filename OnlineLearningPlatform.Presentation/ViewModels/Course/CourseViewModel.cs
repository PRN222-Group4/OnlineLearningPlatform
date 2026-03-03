namespace OnlineLearningPlatform.Presentation.ViewModels.Course
{
    public class CourseViewModel
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public string Level { get; set; } = "Beginner"; // Mặc định nếu chưa có

        public string InstructorName { get; set; } = "Instructor";
        public string Category { get; set; } = "Development";
        public double Rating { get; set; } = 5.0; // Tạm thời hardcode hoặc tính sau
        public int Students { get; set; } = 0;    // Tạm thời hardcode hoặc count sau
        public string Duration { get; set; } = "10h"; // Tạm thời hardcode
    }
}
