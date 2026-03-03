using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class LessonResource
{
    public Guid LessonResourceId { get; set; }

    public Guid LessonItemId { get; set; }

    public string Title { get; set; } = null!;

    public int ResourceType { get; set; }

    public string? ResourceUrl { get; set; }

    public int OrderIndex { get; set; }

    public string? TextContent { get; set; }

    public long? DurationInSeconds { get; set; }

    public bool IsDownloadable { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual LessonItem LessonItem { get; set; } = null!;
}
