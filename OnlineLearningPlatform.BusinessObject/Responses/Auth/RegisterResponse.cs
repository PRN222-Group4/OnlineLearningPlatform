namespace OnlineLearningPlatform.BusinessObject.Responses.Auth
{
    public class RegisterResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public string? Image { get; set; }
    }
}
