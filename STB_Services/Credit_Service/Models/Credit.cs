using System;
using System.Collections.Generic;

namespace Credit_Service.Models;

public partial class Credit
{
    public int CreditId { get; set; }

    public int AccountId { get; set; }

    public decimal CreditAmount { get; set; }

    public int DurationMonths { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Status { get; set; } = null!;

    public string CreditType { get; set; } = null!;

    public string? Cin { get; set; }

    public string? MaritalStatus { get; set; }

    public string AmortizationType { get; set; } = null!;
}
