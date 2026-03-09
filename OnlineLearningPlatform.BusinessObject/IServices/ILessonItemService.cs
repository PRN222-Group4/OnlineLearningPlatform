using OnlineLearningPlatform.BusinessObject.Requests.LessonItem;
using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface ILessonItemService
    {
        Task<ApiResponse> GetLessonItemsByLessonAsync(Guid lessonId);
        Task<ApiResponse> GetLessonItemDetailAsync(Guid lessonItemId);
        Task<ApiResponse> CreateReadingItemAsync(CreateReadingItemRequest request);
        Task<ApiResponse> CreateVideoItemAsync(CreateVideoItemRequest request);
        Task<ApiResponse> CreateQuizItemAsync(CreateQuizItemRequest request);
        Task<ApiResponse> DeleteLessonItemAsync(Guid lessonItemId);
        Task<ApiResponse> CreateWritingItemAsync(CreateWritingItemRequest request);
        Task<ApiResponse> CreateSpeakingItemAsync(CreateSpeakingItemRequest request);
    }
}
