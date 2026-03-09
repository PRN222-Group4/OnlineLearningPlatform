using OnlineLearningPlatform.BusinessObject.Requests.Payment;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Responses.Payment;
using PayOS.Models.Webhooks;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IPaymentService
    {
        /*        Task<ApiResponse> CreatePaymentUrlAsync(CreateNewPaymentRequest request, HttpContext context);
                Task<ApiResponse> PaymentExecuteAsync(IQueryCollection collection);*/
        Task<PaymentResponse> CreatePayOSPaymentAsync(CreateNewPaymentRequest request);
        Task HandlePayOSWebhookAsync(WebhookData data);
        Task ExpirePendingPaymentAsync();

        Task<ApiResponse> SyncPaymentStatusAsync(long orderCode);
        // Return ApiResponse where Result is List<PaymentRecord>
        Task<ApiResponse> GetSuccessfulPaymentsAsync();
        // Return simple list of PaymentRecord DTOs for presentation layer without using ApiResponse wrapper
        Task<ApiResponse> GetSuccessfulPaymentRecordsAsync();
        // Admin helpers
        Task<OnlineLearningPlatform.BusinessObject.Responses.Admin.TopCourseResponse> GetTopCourseByEnrollmentsAsync();
        Task<OnlineLearningPlatform.BusinessObject.Responses.Admin.TopInstructorResponse> GetTopInstructorByStudentsAsync();
    }
}
