using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class Lesson
{
    public Guid LessonId { get; set; }

    public Guid ModuleId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int EstimatedMinutes { get; set; }

    public int OrderIndex { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<LessonItem> LessonItems { get; set; } = new List<LessonItem>();

    public virtual Module Module { get; set; } = null!;

    public virtual ICollection<UserLessonProgress> UserLessonProgresses { get; set; } = new List<UserLessonProgress>();
}
