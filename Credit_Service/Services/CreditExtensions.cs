using Credit_Service.Models;

namespace Credit_Service.Extensions;

public static class CreditExtensions
{
    public static DateOnly GetNextDueDate(this Credit credit)
    {
        var lastPayment = credit.Repayments?
            .Where(r => r.Status == "Completed")
            .MaxBy(r => r.RepaymentDate);

        return (lastPayment?.RepaymentDate ?? credit.StartDate).AddMonths(1);
    }
}