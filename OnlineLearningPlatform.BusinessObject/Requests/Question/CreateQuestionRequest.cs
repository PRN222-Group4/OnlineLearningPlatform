using OnlineLearningPlatform.BusinessObject.Requests.AnswerOption;

namespace OnlineLearningPlatform.BusinessObject.Requests.Question
{
    public class CreateQuestionRequest
    {
        public Guid GradedItemId { get; set; }

        public string Content { get; set; }
        public int Type { get; set; }
        public decimal Points { get; set; }
        public string? Explanation { get; set; }
        public int OrderIndex { get; set; }

        public List<CreateAnswerOptionRequest>? AnswerOptions { get; set; }

    }
}