using OnlineLearningPlatform.BusinessObject.Responses.Admin;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IAdminService
    {
        Task<AdminOverviewResponse> GetOverviewAsync(int recentPayments = 10);
    }
}