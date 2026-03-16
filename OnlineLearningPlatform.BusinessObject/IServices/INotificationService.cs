namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface INotificationService
    {
        Task NotifyAdminNewCourseSubmittedAsync(string courseTitle);
    }
}