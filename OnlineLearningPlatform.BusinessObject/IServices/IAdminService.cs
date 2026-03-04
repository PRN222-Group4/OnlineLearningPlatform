using OnlineLearningPlatform.BusinessObject.Requests.Admin;
using OnlineLearningPlatform.BusinessObject.Responses.Admin;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IAdminService
    {
        Task<AdminOverviewResponse> GetOverviewAsync(int recentPayments = 10);
        Task<AdminUsersResponse> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null);
        Task<AdminUserItem?> GetUserByIdAsync(Guid userId);
        Task<bool> UpdateUserAsync(Guid userId, AdminUpdateUserRequest request);
        Task<bool> SoftDeleteUserAsync(Guid userId);
        Task<bool> ToggleBanUserAsync(Guid userId);
        Task<AdminDashboardResponse> GetDashboardAsync(int year);
    }
}
