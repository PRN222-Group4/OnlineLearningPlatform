using OnlineLearningPlatform.BusinessObject.Requests.Module;
using OnlineLearningPlatform.BusinessObject.Responses;
namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface INotificationService
    {
        Task NotifyAdminNewCourseSubmittedAsync(string courseTitle);

        Task NotifyWalletUpdated(string userId, string studentEmail = "", string courseTitle = "", decimal amount = 0);
    }
}