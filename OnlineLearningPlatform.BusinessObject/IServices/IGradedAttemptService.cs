using OnlineLearningPlatform.BusinessObject.Responses;
using Microsoft.AspNetCore.Http;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IGradedAttemptService
    {
        Task<ApiResponse> StartAttemptAsync(Guid gradedItemId);
        Task<ApiResponse> SubmitShortAnswerAsync(Guid attemptId, Guid questionId, string answer, IFormFile? file);
        Task<ApiResponse> SubmitAttemptAsync(Guid attemptId);
        Task<ApiResponse> GradeAssignmentAsync(Guid attemptId, decimal score);
    }
}
