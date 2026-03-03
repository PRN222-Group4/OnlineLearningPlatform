using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _http;
        private readonly AppSettings _appSettings;
        public AIService(HttpClient http, AppSettings appSettings)
        {
            _appSettings = appSettings;
            _http = http;
        }

        public async Task<string> GetAIResponseAsync(string prompt)
        {
            var requestBody = new
            {
                model = "llama3",
                prompt = prompt,
                stream = false,
                options = new
                {
                    num_ctx = 2048,
                    num_predict = 256,
                    temperature = 0.7
                }
            };

            var response = await _http.PostAsJsonAsync(_appSettings.OllamaAI, requestBody);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
            return jsonResponse.response;
        }
    }
}
