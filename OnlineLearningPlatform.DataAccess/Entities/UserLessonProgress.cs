using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class UserLessonProgress
{
    public Guid LessonProgressId { get; set; }

    public Guid UserId { get; set; }

    public Guid LessonId { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? LastAccessedAt { get; set; }

    public int? LastWatchedSecond { get; set; }

    public int? CompletionPercent { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
