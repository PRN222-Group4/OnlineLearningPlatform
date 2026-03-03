using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class QuestionSubmission
{
    public Guid QuestionSubmissionId { get; set; }

    public Guid GradedAttemptId { get; set; }

    public Guid QuestionId { get; set; }

    public string? AnswerText { get; set; }

    public decimal? Score { get; set; }

    public bool IsAutoGraded { get; set; }

    public string? Feedback { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual GradedAttempt GradedAttempt { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;

    public virtual ICollection<SubmissionAnswerOption> SubmissionAnswerOptions { get; set; } = new List<SubmissionAnswerOption>();
}
