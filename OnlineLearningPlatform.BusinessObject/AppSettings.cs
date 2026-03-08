namespace OnlineLearningPlatform.BusinessObject
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public Logging Logging { get; set; }
        public string AllowedHosts { get; set; }
        public string OllamaAI { get; set; }
        public string SecretToken { get; set; }
        public SMTP SMTP { get; set; }
        public PayOS PayOS { get; set; }
        public AWS AWS { get; set; }
    }
    public class SMTP
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; }
        public string LocalDockerConnection { get; set; }
    }

    public class Logging
    {
        public LogLevel LogLevel { get; set; }
    }

    public class LogLevel
    {
        public string Default { get; set; }
        public string MicrosoftAspNetCore { get; set; }
    }

    public class PayOS
    {
        public string ClientId { get; set; }
        public string ApiKey { get; set; }
        public string ChecksumKey { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
    }

    public class AWS
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string BucketName { get; set; }
        public string CloudFrontDomain { get; set; }
    }
}
