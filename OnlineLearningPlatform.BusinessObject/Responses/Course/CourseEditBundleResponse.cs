namespace OnlineLearningPlatform.BusinessObject.Responses.Course
{
    public class CourseEditBundleResponse
    {
        public global::OnlineLearningPlatform.DataAccess.Entities.Course Course { get; set; } = null!;
        public List<global::OnlineLearningPlatform.DataAccess.Entities.Module> Modules { get; set; } = new();
        public List<global::OnlineLearningPlatform.DataAccess.Entities.Lesson> Lessons { get; set; } = new();
        public List<global::OnlineLearningPlatform.DataAccess.Entities.LessonItem> LessonItems { get; set; } = new();
        public List<global::OnlineLearningPlatform.DataAccess.Entities.LessonResource> LessonResources { get; set; } = new();
        public List<global::OnlineLearningPlatform.DataAccess.Entities.GradedItem> GradedItems { get; set; } = new();
        public List<global::OnlineLearningPlatform.DataAccess.Entities.Question> Questions { get; set; } = new();
        public List<global::OnlineLearningPlatform.DataAccess.Entities.AnswerOption> AnswerOptions { get; set; } = new();
    }
}
