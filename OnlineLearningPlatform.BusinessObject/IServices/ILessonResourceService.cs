using OnlineLearningPlatform.BusinessObject.Requests.LessonResource;
using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface ILessonResourceService
    {
        Task<ApiResponse> CreateLessonResourceAsync(CreateLessonResourceRequest request);
        Task<ApiResponse> GetResourcesByLessonItemAsync(Guid lessonItemId);
        Task<ApiResponse> UpdateLessonResourceAsync(Guid resourceId, UpdateLessonResourceRequest request);
        Task<ApiResponse> DeleteLessonResourceAsync(Guid resourceId);
    }
}
