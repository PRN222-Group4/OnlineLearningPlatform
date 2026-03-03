using Microsoft.AspNetCore.Http;

namespace OnlineLearningPlatform.BusinessObject.Requests.LessonResource
{
    public class UpdateLessonResourceRequest
    {
        public string? Title { get; set; }
        public IFormFile? File { get; set; }
    }
}
