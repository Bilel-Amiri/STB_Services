using System;
using System.Collections.Generic;

namespace Transaction_Service.Model;

public partial class TransactionTemp
{
    public Guid TempTransactionId { get; set; }

    public int AccountId { get; set; }

    public long TargetRib { get; set; }

    public string? TargetEmail { get; set; }

    public decimal Amount { get; set; }

    public string TransactionType { get; set; } = null!;

    public string? Motif { get; set; }

    public byte Status { get; set; }

    public DateTime InitiationDate { get; set; }

    public DateTime? ValidationDate { get; set; }
}
