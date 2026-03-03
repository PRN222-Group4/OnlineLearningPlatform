using OnlineLearningPlatform.BusinessObject.Requests.Module;
using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IModuleService
    {
        Task<ApiResponse> CreateNewModuleForCourseAsync(CreateNewModuleForCourseRequest request);
        Task<ApiResponse> GetModulesByCourseAsync(Guid courseId);
        Task<ApiResponse> GetModuleDetailAsync(Guid moduleId);
        Task<ApiResponse> UpdateModuleAsync(UpdateModuleRequest request);
        Task<ApiResponse> DeleteModuleAsync(Guid moduleId);
    }
}
