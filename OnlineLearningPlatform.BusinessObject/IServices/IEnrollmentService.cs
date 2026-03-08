using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Requests.Enrollment;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IEnrollmentService
    {
        Task<ApiResponse> EnrollStudentDirectlyAsync(Guid courseId);
        Task<ApiResponse> GetStudentEnrollmentsAsync();
        Task<bool> CheckEnrollmentAsync(Guid courseId);

        Task<ApiResponse> CheckUserEnrollmentAsync(Guid userId, Guid courseId);
    }
}
