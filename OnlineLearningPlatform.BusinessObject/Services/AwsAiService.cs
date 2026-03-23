using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class AwsAiService : IAwsAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public AwsAiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            _apiKey = configuration["AwsAi:ApiKey"] ?? "";
            var rawBaseUrl = configuration["AwsAi:BaseUrl"] ?? "";

            if (string.IsNullOrWhiteSpace(_apiKey)) throw new ArgumentException("AwsAi:ApiKey đang trống!");
            if (string.IsNullOrWhiteSpace(rawBaseUrl)) throw new ArgumentException("AwsAi:BaseUrl đang trống!");

            _baseUrl = rawBaseUrl.StartsWith("http") ? rawBaseUrl : $"https://{rawBaseUrl}";
        }

        public async Task<ApiResponse> EvaluateWritingAsync(string prompt, string essayContent)
        {
            var response = new ApiResponse();
            try
            {
                var requestBody = new { section_id = "section-001", user_id = "test-user-001", essay_content = essayContent, task_type = "Task 2", prompt = prompt ?? "Write an essay about the given topic.", word_count = 250 };
                var writingEndpoint = $"{_baseUrl.TrimEnd('/')}/writing/evaluate";

                var request = new HttpRequestMessage(HttpMethod.Post, writingEndpoint)
                {
                    Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
                };
                request.Headers.Add("x-api-key", _apiKey);
                request.Headers.Add("Accept", "*/*");

                var httpResponse = await _httpClient.SendAsync(request);
                var responseString = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode) return response.SetBadRequest(message: $"[AWS Lỗi Writing]: {responseString}");

                var aiData = JsonSerializer.Deserialize<JsonElement>(responseString);
                return response.SetOk(new AiEvaluationResult { Score = aiData.GetProperty("overall_band").GetDecimal(), Feedback = responseString });
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: $"[Lỗi C# Writing]: {ex.Message}");
            }
        }

        public async Task<ApiResponse> EvaluateSpeakingAsync(string prompt, IFormFile audioFile)
        {
            var response = new ApiResponse();
            try
            {
                // ==========================================
                // PHASE 1: XIN PRESIGNED URL
                // ==========================================
                var presignEndpoint = $"{_baseUrl.TrimEnd('/')}/upload/audio";
                var presignBody = new { user_id = "test-user-001", session_id = "session-001", filename = audioFile.FileName.Replace(" ", "-"), content_type = audioFile.ContentType, upload_type = "speaking_audio" };

                var reqPhase1 = new HttpRequestMessage(HttpMethod.Post, presignEndpoint)
                {
                    Content = new StringContent(JsonSerializer.Serialize(presignBody), Encoding.UTF8, "application/json")
                };
                reqPhase1.Headers.Add("x-api-key", _apiKey);
                reqPhase1.Headers.Add("Accept", "*/*");

                var resPhase1 = await _httpClient.SendAsync(reqPhase1);
                var strPhase1 = await resPhase1.Content.ReadAsStringAsync();

                if (!resPhase1.IsSuccessStatusCode) return response.SetBadRequest(message: $"[Phase 1 Failed]: {strPhase1}");

                var dataPhase1 = JsonSerializer.Deserialize<JsonElement>(strPhase1);
                var uploadUrl = dataPhase1.GetProperty("upload_url").GetString();
                var getUrl = dataPhase1.GetProperty("get_url").GetString();

                if (string.IsNullOrEmpty(uploadUrl)) return response.SetBadRequest(message: "[Phase 1 Failed]: AWS không trả về upload_url");

                // ==========================================
                // PHASE 2: ĐẨY FILE LÊN S3
                // ==========================================
                using var fileStream = audioFile.OpenReadStream();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(audioFile.ContentType);

                var reqPhase2 = new HttpRequestMessage(HttpMethod.Put, uploadUrl) { Content = streamContent };
                // Chú ý: TUYỆT ĐỐI KHÔNG add x-api-key vào đây vì S3 sẽ văng lỗi Signature Does Not Match!

                var resPhase2 = await _httpClient.SendAsync(reqPhase2);
                if (!resPhase2.IsSuccessStatusCode) return response.SetBadRequest(message: $"[Phase 2 Failed S3]: {await resPhase2.Content.ReadAsStringAsync()}");

                // ==========================================
                // PHASE 3: GỌI AI CHẤM ĐIỂM
                // ==========================================
                var evaluateEndpoint = $"{_baseUrl.TrimEnd('/')}/speaking/evaluate";
                var evalBody = new { answerId = "answer-001", session_id = "session-001", user_id = "test-user-001", audio_url = getUrl, task_type = "Part 2", prompt = prompt ?? "Describe a memorable trip you had.", duration_seconds = 40 };

                var reqPhase3 = new HttpRequestMessage(HttpMethod.Post, evaluateEndpoint)
                {
                    Content = new StringContent(JsonSerializer.Serialize(evalBody), Encoding.UTF8, "application/json")
                };
                reqPhase3.Headers.Add("x-api-key", _apiKey);
                reqPhase3.Headers.Add("Accept", "*/*");

                var resPhase3 = await _httpClient.SendAsync(reqPhase3);
                var strPhase3 = await resPhase3.Content.ReadAsStringAsync();

                if (!resPhase3.IsSuccessStatusCode) return response.SetBadRequest(message: $"[Phase 3 Failed AI]: {strPhase3}");

                var aiData = JsonSerializer.Deserialize<JsonElement>(strPhase3);
                return response.SetOk(new AiEvaluationResult { Score = aiData.GetProperty("overall_band").GetDecimal(), Feedback = strPhase3 });
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: $"[C# Exception]: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public async Task<ApiResponse> GenerateQuizFromPdfAsync(IFormFile pdfFile)
        {
            var response = new ApiResponse();
            try
            {
                // ==========================================
                // PHASE 1: XIN PRESIGNED URL (DOCUMENT)
                // ==========================================
                var presignEndpoint = $"{_baseUrl.TrimEnd('/')}/upload/document";
                var presignBody = new
                {
                    user_id = "test-user-001",
                    session_id = "flashcard-session-001",
                    filename = pdfFile.FileName.Replace(" ", "-"),
                    content_type = "application/pdf",
                    upload_type = "flashcard_pdf"
                };

                var reqPhase1 = new HttpRequestMessage(HttpMethod.Post, presignEndpoint)
                {
                    Content = new StringContent(JsonSerializer.Serialize(presignBody), Encoding.UTF8, "application/json")
                };
                reqPhase1.Headers.Add("x-api-key", _apiKey);
                reqPhase1.Headers.Add("Accept", "*/*");

                var resPhase1 = await _httpClient.SendAsync(reqPhase1);
                var strPhase1 = await resPhase1.Content.ReadAsStringAsync();
                if (!resPhase1.IsSuccessStatusCode) return response.SetBadRequest($"[Phase 1 Failed]: {strPhase1}");

                var dataPhase1 = JsonSerializer.Deserialize<JsonElement>(strPhase1);
                var uploadUrl = dataPhase1.GetProperty("upload_url").GetString();
                var getUrl = dataPhase1.GetProperty("get_url").GetString();

                // ==========================================
                // PHASE 2: ĐẨY FILE PDF LÊN S3
                // ==========================================
                using var fileStream = pdfFile.OpenReadStream();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

                var reqPhase2 = new HttpRequestMessage(HttpMethod.Put, uploadUrl) { Content = streamContent };
                var resPhase2 = await _httpClient.SendAsync(reqPhase2);
                if (!resPhase2.IsSuccessStatusCode) return response.SetBadRequest($"[Phase 2 Failed]: {await resPhase2.Content.ReadAsStringAsync()}");

                // ==========================================
                // PHASE 3: GỌI AI GENERATE FLASHCARDS
                // ==========================================
                var generateEndpoint = $"{_baseUrl.TrimEnd('/')}/flashcards/generate";
                var evalBody = new
                {
                    set_id = "flashcard-test-001",
                    user_id = "test-user-001",
                    document_id = "doc-001",
                    pdf_url = getUrl,
                    num_cards = 10,
                    difficulty = "MEDIUM"
                };

                var reqPhase3 = new HttpRequestMessage(HttpMethod.Post, generateEndpoint)
                {
                    Content = new StringContent(JsonSerializer.Serialize(evalBody), Encoding.UTF8, "application/json")
                };
                reqPhase3.Headers.Add("x-api-key", _apiKey);
                reqPhase3.Headers.Add("Accept", "*/*");

                var resPhase3 = await _httpClient.SendAsync(reqPhase3);
                var strPhase3 = await resPhase3.Content.ReadAsStringAsync();

                if (!resPhase3.IsSuccessStatusCode) return response.SetBadRequest($"[Phase 3 Failed]: {strPhase3}");

                return response.SetOk(strPhase3);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest($"[C# Exception]: {ex.Message}");
            }
        }
    }
}