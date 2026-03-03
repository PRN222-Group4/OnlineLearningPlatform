using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class GradedItem
{
    public Guid GradedItemId { get; set; }

    public Guid LessonItemId { get; set; }

    public int MaxScore { get; set; }

    public bool IsAutoGraded { get; set; }

    public int GradedItemType { get; set; }

    public string? SubmissionGuidelines { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual GradedAttempt? GradedAttempt { get; set; }

    public virtual LessonItem LessonItem { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
