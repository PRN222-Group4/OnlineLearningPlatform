namespace OnlineLearningPlatform.BusinessObject.Responses.Message
{
    public class MessageResponse
    {
        public Guid MessageId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }

        public string SenderName { get; set; } = string.Empty;
    }
}