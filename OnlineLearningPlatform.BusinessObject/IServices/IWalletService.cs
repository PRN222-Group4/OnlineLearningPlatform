using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IWalletService
    {
        Task<ApiResponse> GetMyWalletAsync();

        Task<ApiResponse> RequestWithdrawalAsync(decimal amount, string bankInfo);

        Task<ApiResponse> GetPendingPayoutsAsync();

        Task<ApiResponse> ApprovePayoutAsync(Guid walletId);

        Task<ApiResponse> GetPlatformRevenueAsync();

        Task<ApiResponse> GetCashflowReportAsync();
    }
}