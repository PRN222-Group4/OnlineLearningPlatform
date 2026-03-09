using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.DataAccess.Entities
{
    public partial class Certificate
    {
        [Key]
        public Guid CertificateId { get; set; }

        public Guid UserId { get; set; }

        public Guid CourseId { get; set; }

        public DateTime IssueDate { get; set; }

        [MaxLength(50)]
        public string CertificateCode { get; set; } = null!;

        public string? CertificateUrl { get; set; }

        public bool IsDeleted { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}