using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class GradedAttempt
{
    public Guid GradedAttemptId { get; set; }

    public Guid UserId { get; set; }

    public Guid GradedItemId { get; set; }

    public int AttemptNumber { get; set; }

    public int Status { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? GradedAt { get; set; }

    public decimal? Score { get; set; }

    public int MaxScore { get; set; }

    public bool IsPassed { get; set; }

    public string? SubmittedText { get; set; }

    public string? FileUrl { get; set; }

    public string? AudioUrl { get; set; }

    public string? Feedback { get; set; }

    public Guid? GradedBy { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual GradedItem GradedAttemptNavigation { get; set; } = null!;

    public virtual ICollection<QuestionSubmission> QuestionSubmissions { get; set; } = new List<QuestionSubmission>();

    public virtual User User { get; set; } = null!;
}
