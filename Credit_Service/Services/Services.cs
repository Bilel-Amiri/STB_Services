using System.Linq;
using Credit_Service.DTOs;
using Credit_Service.Interfaces;
using Credit_Service.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Credit_Service.Services;

namespace Credit_Service.Services
{
    public class CreditService : ICreditStatusService, ICreditSimulationService, ICreditService, IDisposable
    {
        private readonly ICreditRepository _creditRepository;
        private readonly IUserServiceClient _userServiceClient;
        private readonly ILogger<CreditService> _logger;

        public CreditService(
            ICreditRepository creditRepository,
            IUserServiceClient userServiceClient,
            ILogger<CreditService> logger)
        {
            _creditRepository = creditRepository;
            _userServiceClient = userServiceClient;
            _logger = logger;
        }

        public async Task<CreditStatusResponse> GetCreditStatusAsync(int clientId, int accountID)
        {
            if (clientId <= 0)
                throw new ArgumentException("Invalid client ID", nameof(clientId));

            if (accountID <= 0)
                throw new ArgumentException("Account ID must be positive", nameof(accountID));

            var account = await _userServiceClient.GetAccountAsync(accountID);
            if (account == null)
            {
                throw new Exception($"Account {accountID} not found");
            }

            var userInfo = await _userServiceClient.GetUserInfoAsync(clientId);
            if (userInfo == null)
            {
                throw new Exception($"User info not found for client {clientId}");
            }

            var credits = await _creditRepository.GetActiveCreditsByClientAsync(accountID);

            // Safely get the minimum due date as DateTime
            DateOnly? upcomingDueDate = credits.Any()
                ? credits.Min(c => GetNextDueDate(c))
                : null;

            var creditDetails = credits.Select(c => new CreditDetailDto(
                c.CreditId,
                "CR-" + c.CreditId.ToString("D8"),
                c.CreditAmount,
                c.RemainingBalance,
                c.InterestRate,
                c.StartDate,          // Already DateTime
                c.EndDate,            // Already DateTime
                GetNextDueDate(c),    // Now returns DateTime
                c.Status))
            .ToList();

            return new CreditStatusResponse(
                accountID,
                $"{userInfo.FirstName} {userInfo.LastName}",
                credits.Sum(c => c.CreditAmount),
                credits.Sum(c => c.RemainingBalance),
                upcomingDueDate,
                creditDetails);
        }





       




        public async Task<CreditCreationResponse> CreateCreditAsync(int accountId, CreditCreationRequest request)
        {
            var account = await _userServiceClient.GetAccountAsync(accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            var credit = new Credit
            {
                AccountId = accountId,
                CreditAmount = request.Amount,
                InterestRate = request.AnnualInterestRate,
                DurationMonths = request.DurationMonths,
                StartDate = request.StartDate,
                EndDate = request.StartDate.AddMonths(request.DurationMonths),
                RemainingBalance = request.Amount,
                Status = "Active"
            };

            await _creditRepository.AddCreditAsync(credit);

            string contractNumber = $"CR-{credit.CreditId:D8}";

            return new CreditCreationResponse(
                CreditId: credit.CreditId,
                ContractNumber: contractNumber,
                ClientId: credit.AccountId,
                Amount: credit.CreditAmount,
                MonthlyPayment: CalculateMonthlyPayment(credit),
                StartDate: credit.StartDate,
                EndDate: credit.EndDate,
                Status: credit.Status,
                PaymentSchedule: GeneratePaymentSchedule(credit));
        }

        private decimal CalculateTotalInterest(Credit credit)
        {
            decimal totalInterest = credit.CreditAmount * (credit.InterestRate / 100) * (credit.DurationMonths / 12m);
            decimal paidInterest = credit.Repayments
                .Where(r => r.Status == "Completed")
                .Sum(r => r.AmountRepaid - (r.AmountRepaid / (1 + (credit.InterestRate / 100))));

            return totalInterest - paidInterest;
        }

        private DateOnly GetNextDueDate(Credit credit)
        {
            // Safe null check for Repayments
            var lastPayment = credit.Repayments?
                .Where(r => r.Status == "Completed")
                .OrderByDescending(r => r.RepaymentDate)
                .FirstOrDefault();

            // Calculate base date (use StartDate if no payments)
            var baseDate = lastPayment?.RepaymentDate ?? credit.StartDate;

            // Return next due date (1 month after base date)
            return baseDate.AddMonths(1);
        }


        private (decimal MonthlyPayment, decimal TotalInterest) CalculateInstallment(
            decimal amount, int months, decimal annualRate)
        {
            decimal monthlyRate = annualRate / 12 / 100;
            decimal factor = (decimal)Math.Pow(1 + (double)monthlyRate, months);
            decimal payment = amount * monthlyRate * factor / (factor - 1);
            return (Math.Round(payment, 2), Math.Round(payment * months - amount, 2));
        }

        private decimal CalculateMonthlyPayment(Credit credit)
        {
            return CalculateInstallment(credit.CreditAmount, credit.DurationMonths, credit.InterestRate).MonthlyPayment;
        }

        private List<DueDateAlert> GeneratePaymentSchedule(Credit credit)
        {
            var schedule = new List<DueDateAlert>();
            var monthlyPayment = CalculateMonthlyPayment(credit);

            for (int i = 1; i <= credit.DurationMonths; i++)
            {
                schedule.Add(new DueDateAlert(
                    InstallmentNumber: i,
                    DueDate: credit.StartDate.AddMonths(i),
                    Amount: monthlyPayment));
            }

            return schedule;
        }

        private decimal CalculateAPR(decimal nominalRate)
        {
            
            return nominalRate * 1.1m;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

       


    }
}
public class InterestRateConfiguration
{
    public decimal BaseRate { get; set; } = 5.0m; 
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

