using Microsoft.AspNetCore.Http;

namespace OnlineLearningPlatform.BusinessObject.Requests.Course
{
    public class CreateNewCourseRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile? ImageFile { get; set; }
        public decimal Price { get; set; }
        public int Level { get; set; }
        public Guid LanguageId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Default: English
    }
}