namespace OnlineLearningPlatform.BusinessObject.Requests.LessonItem
{
    public class CreateReadingItemRequest
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int OrderIndex { get; set; }
    }
}