using Microsoft.AspNetCore.Http;

namespace OnlineLearningPlatform.BusinessObject.Requests.Course
{
    public class UpdateCourseRequest
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string? Subtitle { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Level { get; set; }
        public Guid LanguageId { get; set; }
        public string? Tags { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
