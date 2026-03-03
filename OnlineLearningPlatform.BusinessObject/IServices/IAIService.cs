namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IAIService
    {
        Task<string> GetAIResponseAsync(string prompt);
    }
}
