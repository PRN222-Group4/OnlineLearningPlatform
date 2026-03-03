using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.DataAccess.Entities;

public partial class Wallet
{
    public Guid WalletId { get; set; }

    public Guid UserId { get; set; }

    public decimal Balance { get; set; }

    public decimal PendingBalance { get; set; }

    public decimal TotalEarnings { get; set; }

    public decimal TotalWithdrawn { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
