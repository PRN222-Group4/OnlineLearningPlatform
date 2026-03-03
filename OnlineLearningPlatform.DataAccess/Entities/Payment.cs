namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public Guid UserId { get; set; }

    public Guid? CourseId { get; set; }

    public Guid? EnrollmentId { get; set; }

    public long OrderCode { get; set; }

    public string PaymentLinkId { get; set; } = null!;

    public string? CheckoutUrl { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string? Reference { get; set; }

    public string? CounterAccountNumber { get; set; }

    public string? CounterAccountName { get; set; }

    public string? CounterAccountBankName { get; set; }

    public string Method { get; set; } = null!;

    public int Status { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? RawWebhookData { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public int Type { get; set; }

    public virtual Course? Course { get; set; }

    public virtual Enrollment? Enrollment { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
