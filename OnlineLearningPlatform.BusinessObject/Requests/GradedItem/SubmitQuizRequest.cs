

namespace OnlineLearningPlatform.BusinessObject.Requests.GradedItem
{
    public class SubmitQuizRequest
    {
        public Guid GradedItemId { get; set; }
        public List<AnswerSubmission> Answers { get; set; }
    }
    public class QuestionAnswerRequest
    {
        public Guid QuestionId { get; set; }
        public List<Guid> SelectedAnswerOptionIds { get; set; }
    }

    public class AnswerSubmission
    {
        public Guid QuestionId { get; set; }
        public List<Guid> SelectedAnswerOptionIds { get; set; } = new List<Guid>();
    }
}
