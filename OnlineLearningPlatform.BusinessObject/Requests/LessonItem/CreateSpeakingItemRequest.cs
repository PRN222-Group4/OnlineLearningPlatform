namespace OnlineLearningPlatform.BusinessObject.Requests.LessonItem
{
    public class CreateSpeakingItemRequest
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; } = null!;
        public string Prompt { get; set; } = null!;
        public int OrderIndex { get; set; }
    }
}