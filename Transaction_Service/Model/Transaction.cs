using System;
using System.Collections.Generic;

namespace Transaction_Service.Model;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int AccountId { get; set; }

    public decimal Amount { get; set; }

    public string TransactionType { get; set; } = null!;

    public DateTime? TransactionDate { get; set; }

    public int? TargetAccountId { get; set; }

    public long? TargetRib { get; set; }

    public string? TargetEmail { get; set; }

    public string? Motif { get; set; }
}
