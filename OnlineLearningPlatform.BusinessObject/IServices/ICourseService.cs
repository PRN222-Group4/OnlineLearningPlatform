using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.BusinessObject.Requests.Course;
using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface ICourseService
    {
        Task<ApiResponse> CreateNewCourseAsync(CreateNewCourseRequest request);
        Task<ApiResponse> GetAllCourseAsync();
        Task<ApiResponse> GetCourseDetailAsync(Guid courseId);
        Task<ApiResponse> GetAllCourseForAdminAsync(int status);
        Task<ApiResponse> GetCoursesByInstructorAsync();
        Task<ApiResponse> GetEnrolledCoursesForStudentAsync();
        Task<ApiResponse> ApproveCourseAsync(ApproveCourseRequest request);
        Task<ApiResponse> SubmitCourseForReviewAsync(Guid courseId);
        Task<ApiResponse> GetCoursesByStatusAsync(int status);
        Task<ApiResponse> GetCourseByIdAsync(Guid courseId);
        Task<ApiResponse> UpdateCourseAsync(UpdateCourseRequest request);
        Task<ApiResponse> DeleteCourseAsync(Guid courseId);
        Task<ApiResponse> GetCourseDetailForStudentAsync(Guid courseId);

        // Wizard flow methods
        Task<ApiResponse> GetCourseForEditAsync(Guid courseId);
        Task<ApiResponse> ValidateAndSubmitForReviewAsync(Guid courseId);
        Task<ApiResponse> GetPendingCoursesForAdminAsync();
    }
}
