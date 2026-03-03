using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class Question
{
    public Guid QuestionId { get; set; }

    public Guid GradedItemId { get; set; }

    public string Content { get; set; } = null!;

    public int Type { get; set; }

    public decimal Points { get; set; }

    public int OrderIndex { get; set; }

    public bool? IsRequired { get; set; }

    public string? Explanation { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();

    public virtual GradedItem GradedItem { get; set; } = null!;

    public virtual ICollection<QuestionSubmission> QuestionSubmissions { get; set; } = new List<QuestionSubmission>();
}
