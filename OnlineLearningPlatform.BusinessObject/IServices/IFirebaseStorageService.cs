using OnlineLearningPlatform.DataAccess.Entities;
using Microsoft.AspNetCore.Http;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IFirebaseStorageService
    {
        Task<string> UploadCourseImage(string courseName, IFormFile? file);
        Task<string> UploadUserImage(string userName, IFormFile? file);
        Task<string> UploadQuestionSubmissionFile(IFormFile? file);
        Task<(string Url, int Type)> UploadLessonResourceAsync(
           Guid lessonId,
           string courseName,
           IFormFile file);
    }
}
