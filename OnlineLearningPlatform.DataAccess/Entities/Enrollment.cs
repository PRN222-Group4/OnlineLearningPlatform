using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class Enrollment
{
    public Guid EnrollmentId { get; set; }

    public Guid UserId { get; set; }

    public Guid CourseId { get; set; }

    public int Status { get; set; }

    public decimal ProgressPercent { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? EnrolledAt { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User User { get; set; } = null!;
}
