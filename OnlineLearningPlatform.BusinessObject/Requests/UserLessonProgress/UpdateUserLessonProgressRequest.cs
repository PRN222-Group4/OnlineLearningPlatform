
namespace OnlineLearningPlatform.BusinessObject.Requests.UserLessonProgress
{
    public class UpdateUserLessonProgressRequest
    {
        public Guid LessonId { get; set; }
        public int? LastWatchedSecond { get; set; }
        public int? CompletionPercent { get; set; }
    }
}
