using Microsoft.AspNetCore.Http;
namespace OnlineLearningPlatform.BusinessObject.Requests.LessonItem
{
    public class CreateVideoItemRequest
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; } = null!;
        public IFormFile? VideoFile { get; set; }
        public string? VideoUrl { get; set; }
        public int VideoSourceType { get; set; }
        public int OrderIndex { get; set; }
    }
}