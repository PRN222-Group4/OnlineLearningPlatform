using OnlineLearningPlatform.BusinessObject.Requests.Question;
using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IQuestionService
    {
        Task<ApiResponse> CreateQuestionAsync(CreateQuestionRequest request);
    }
}
