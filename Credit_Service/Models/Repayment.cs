using System;
using System.Collections.Generic;

namespace Credit_Service.Models;

public partial class Repayment
{
    public int RepaymentId { get; set; }

    public int CreditId { get; set; }

    public decimal AmountRepaid { get; set; }

    public DateOnly RepaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Credit Credit { get; set; } = null!;
}
