namespace OnlineLearningPlatform.BusinessObject.Requests.GradedItem
{
    public class QuizAnswerSubmitModel
    {
        public Guid QuestionId { get; set; }
        public Guid SelectedOptionId { get; set; }
    }
}