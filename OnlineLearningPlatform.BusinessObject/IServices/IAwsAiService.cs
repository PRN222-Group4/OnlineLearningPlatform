using Microsoft.AspNetCore.Http;
using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IAwsAiService
    {
        Task<ApiResponse> EvaluateWritingAsync(string prompt, string essayContent);

        Task<ApiResponse> EvaluateSpeakingAsync(string prompt, IFormFile audioFile);

        Task<ApiResponse> GenerateQuizFromPdfAsync(IFormFile pdfFile);
    }

    public class AiEvaluationResult
    {
        public decimal Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }
}