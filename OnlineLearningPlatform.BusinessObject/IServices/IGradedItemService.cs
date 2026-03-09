using OnlineLearningPlatform.BusinessObject.Requests.GradedItem;
using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IGradedItemService
    {
        Task<ApiResponse> SubmitQuizAsync(SubmitQuizRequest request);
    }
}
