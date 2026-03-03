using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Requests.UserLessonProgress;   
namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IUserLessonProgressService
    {
        Task<ApiResponse> StartOrUpdateProgressAsync(UpdateUserLessonProgressRequest request);
        Task<ApiResponse> MarkLessonCompletedAsync(Guid lessonId);
        Task<ApiResponse> GetLessonProgressAsync(Guid lessonId);
        Task<ApiResponse> GetLessonProgressByUserAsync(Guid userId);
    }
}
