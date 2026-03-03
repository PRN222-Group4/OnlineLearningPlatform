namespace OnlineLearningPlatform.BusinessObject.DTOs
{
    public class PasswordDTO
    {
        public byte[] PasswordHash { get; set; } = new byte[32];
        public byte[] PasswordSalt { get; set; } = new byte[32];
    }
}
