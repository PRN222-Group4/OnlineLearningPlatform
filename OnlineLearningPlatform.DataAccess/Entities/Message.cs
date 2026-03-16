using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.DataAccess.Entities
{
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }

        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}