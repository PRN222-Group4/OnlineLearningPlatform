using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IWalletService
    {
        Task<ApiResponse> GetMyWalletAsync();
    }
}