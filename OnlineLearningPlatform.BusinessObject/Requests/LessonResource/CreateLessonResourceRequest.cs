using Microsoft.AspNetCore.Http;

namespace OnlineLearningPlatform.BusinessObject.Requests.LessonResource
{
    public class CreateLessonResourceRequest
    {
        public Guid LessonItemId { get; set; }
        public string Title { get; set; }
        public string? TextContent { get; set; }
        public IFormFile? File { get; set; }
        public bool IsDownloadable { get; set; }
        public int OrderIndex { get; set; }
    }
}
