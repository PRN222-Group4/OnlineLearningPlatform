namespace OnlineLearningPlatform.BusinessObject.Responses.LessonResource
{
    public class LessonResourceResponse
    {
        public Guid LessonResourceId { get; set; }
        public string Title { get; set; }
        public string ResourceUrl { get; set; }
        public int ResourceType { get; set; }
        public Guid LessonId { get; set; }
    }
}