using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class Course
{
    public Guid CourseId { get; set; }

    public Guid LanguageId { get; set; }

    public string Title { get; set; } = null!;

    public string? Subtitle { get; set; }

    public string? Description { get; set; }

    public string? Image { get; set; }

    public int Status { get; set; }

    public decimal Price { get; set; }

    public string? RejectReason { get; set; }

    public int Level { get; set; }

    public string? Tags { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? RejectedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Language Language { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
