using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class WalletTransaction
{
    public Guid WalletTransactionId { get; set; }

    public Guid WalletId { get; set; }

    public decimal Amount { get; set; }

    public int TransactionType { get; set; }

    public string? Description { get; set; }

    public decimal BalanceAfterTransaction { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? PaymentId { get; set; }

    public virtual Payment? Payment { get; set; } = null!;

    public virtual Wallet Wallet { get; set; } = null!;
}
