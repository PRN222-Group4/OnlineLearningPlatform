using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface ICertificateService
    {
        Task<ApiResponse> GetMyCertificatesAsync(Guid userId);

        Task<ApiResponse> VerifyCertificateAsync(string certificateCode);
    }
}