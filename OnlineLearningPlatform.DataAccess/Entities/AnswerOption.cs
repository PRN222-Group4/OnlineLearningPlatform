using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class AnswerOption
{
    public Guid AnswerOptionId { get; set; }

    public Guid QuestionId { get; set; }

    public string Text { get; set; } = null!;

    public string? Explanation { get; set; }

    public int OrderIndex { get; set; }

    public bool IsCorrect { get; set; }

    public decimal Weight { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual ICollection<SubmissionAnswerOption> SubmissionAnswerOptions { get; set; } = new List<SubmissionAnswerOption>();
}
