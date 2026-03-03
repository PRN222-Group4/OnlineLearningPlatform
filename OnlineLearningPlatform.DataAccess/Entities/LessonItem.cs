using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class LessonItem
{
    public Guid LessonItemId { get; set; }

    public Guid LessonId { get; set; }

    public int Type { get; set; }

    public int OrderIndex { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual GradedItem? GradedItem { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual ICollection<LessonResource> LessonResources { get; set; } = new List<LessonResource>();
}
