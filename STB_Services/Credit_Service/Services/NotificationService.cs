using Credit_Service.DTOs;
using Credit_Service.Interfaces;
using Credit_Service.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Credit_Service.Services;

namespace Credit_Service.Services
{
    public class NotificationService : INotificationService, IDisposable
    {
        private const int DaysBeforeReminder = 3;
        private const int NotificationHour = 9;

        private readonly ICreditRepository _creditRepository;
        private readonly IUserServiceClient _userServiceClient;
        private readonly INotificationSender _notificationSender;
        private readonly ILogger<NotificationService> _logger;
        private readonly Timer _notificationTimer;
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(
            ICreditRepository creditRepository,
            IUserServiceClient userServiceClient,
            INotificationSender notificationSender,
            ILogger<NotificationService> logger,
             INotificationRepository notificationRepository)
        {
            _creditRepository = creditRepository;
            _userServiceClient = userServiceClient;
            _notificationSender = notificationSender;
            _notificationRepository = notificationRepository;
            _logger = logger;
            _notificationTimer = new Timer(_ => _ = CheckDueDatesAndNotifyAsync(), null, Timeout.Infinite, 0);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Notification Service starting...");
            _notificationTimer.Change(CalculateNextRunTime(), TimeSpan.FromHours(24));
            await CheckDueDatesAndNotifyAsync();
            _logger.LogInformation("Notification Service started");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Notification Service stopping...");
            _notificationTimer?.Change(Timeout.Infinite, 0);
            _logger.LogInformation("Notification Service stopped");
            return Task.CompletedTask;
        }

        public async Task<NotificationCheckResult> CheckDueDatesAndNotifyAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var credits = await _creditRepository.GetActiveCreditsWithRepaymentsAsync();

            var result = new NotificationCheckResult
            {
                NotificationsCreated = 0,
                UpcomingCount = 0,
                OverdueCount = 0
            };

            foreach (var credit in credits)
            {
                var nextDueDate = CalculateNextDueDate(credit);
                _logger.LogInformation($"Credit ID: {credit.CreditId}, StartDate: {credit.StartDate}, Repayments: {credit.Repayments?.Count ?? 0}, Completed: {credit.Repayments?.Count(r => r.Status == "Completed")}");
                _logger.LogInformation($"NextDueDate: {nextDueDate}, Today: {today}, ShouldNotifyUpcoming: {ShouldNotifyForUpcoming(nextDueDate, today)}, IsOverdue: {IsOverdue(nextDueDate, today)}");



                if (ShouldNotifyForUpcoming(nextDueDate, today))
                {
                    _logger.LogInformation($"Upcoming notification triggered for Credit ID: {credit.CreditId}");
                    result = result with { UpcomingCount = result.UpcomingCount + 1 };
                    await CreateNotification(credit, "Upcoming", nextDueDate);
                    result = result with { NotificationsCreated = result.NotificationsCreated + 1 };
                }

                if (IsOverdue(nextDueDate, today))
                {
                    _logger.LogInformation($"Overdue notification triggered for Credit ID: {credit.CreditId}");
                    result = result with { OverdueCount = result.OverdueCount + 1 };
                    await CreateNotification(credit, "Overdue", nextDueDate);
                    result = result with { NotificationsCreated = result.NotificationsCreated + 1 };
                }
            }

            return result;
        }





        private async Task CreateNotification(Credit credit, string notificationType, DateOnly dueDate)
        {
            var amount = CalculateDueAmount(credit);

            var message = notificationType == "Upcoming"
                ? $"Payment of {amount} DZD due on {dueDate:dd-MM-yyyy}"
                : $"OVERDUE: Payment of {amount} DZD was due on {dueDate:dd-MM-yyyy}";

            _logger.LogInformation($"Creating {notificationType} notification for AccountId: {credit.AccountId} | Message: {message}");

            await SaveNotification(
                credit.AccountId,
                message,
                notificationType == "Upcoming" ? "Pending" : "Overdue",
                dueDate.ToDateTime(TimeOnly.MinValue)
            );
        }





        private bool ShouldNotifyForUpcoming(DateOnly dueDate, DateOnly today)
        {
            return (dueDate.DayNumber - today.DayNumber) is >= 0 and <= 3;
        }

        private bool IsOverdue(DateOnly dueDate, DateOnly today)
        {
            return dueDate < today;
        }




        private async Task ProcessCreditNotification(Credit credit, DateOnly today)
        {
            try
            {
                // Using AccountId instead of ClientId since that's what your model has
                var account = await _userServiceClient.GetAccountAsync(credit.AccountId);
                var userInfo = await _userServiceClient.GetUserInfoAsync(credit.AccountId);

                if (account == null || userInfo == null)
                {
                    _logger.LogWarning("Missing account/user info for credit {CreditId}", credit.CreditId);
                    return;
                }

                var nextDueDate = CalculateNextDueDate(credit);
                var isOverdue = nextDueDate < today;
                var dueAmount = CalculateDueAmount(credit);

                var notification = new CreditNotification(
                    credit.AccountId,  // Using AccountId instead of ClientId
                    $"{userInfo.FirstName} {userInfo.LastName}",
                    userInfo.Email,
                    credit.CreditId,
                    GenerateContractNumber(credit.CreditId),
                    nextDueDate,
                    dueAmount,
                    isOverdue ? "Overdue" : "Upcoming");

                await _notificationSender.SendAsync(notification);

                _logger.LogInformation("Sent {NotificationType} notification for credit {CreditId}",
                    notification.NotificationType, credit.CreditId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing credit {CreditId}", credit.CreditId);
            }
        }

        private DateOnly CalculateNextDueDate(Credit credit)
        {
            // Safely handle null Repayments collection
            var lastPayment = credit.Repayments?
                .Where(r => r.Status == "Completed")
                .OrderByDescending(r => r.RepaymentDate)
                .FirstOrDefault();

            return (lastPayment?.RepaymentDate ?? credit.StartDate).AddMonths(1);
        }

        private decimal CalculateDueAmount(Credit credit)
        {
            var monthlyPayment = CalculateMonthlyPayment(credit);
            return Math.Min(credit.RemainingBalance, monthlyPayment);
        }

        private decimal CalculateMonthlyPayment(Credit credit)
        {
            decimal monthlyRate = credit.InterestRate / 12 / 100;
            double exponent = Math.Pow(1 + (double)monthlyRate, credit.DurationMonths);
            decimal factor = (decimal)exponent;
            return credit.CreditAmount * monthlyRate * factor / (factor - 1);
        }

        private string GenerateContractNumber(int creditId) => $"CR-{creditId:D8}";

        private TimeSpan CalculateNextRunTime()
        {
            var now = DateTime.Now;
            var nextRun = now.Hour < NotificationHour
                ? new DateTime(now.Year, now.Month, now.Day, NotificationHour, 0, 0)
                : new DateTime(now.Year, now.Month, now.Day, NotificationHour, 0, 0).AddDays(1);
            return nextRun - now;
        }

        public void Dispose()
        {
            _notificationTimer?.Dispose();
            GC.SuppressFinalize(this);
        }


        public async Task<IEnumerable<UpcomingNotificationDto>> GetUpcomingNotificationsAsync(int accountId, int daysAhead)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var credits = await _creditRepository.GetCreditsDueBetweenAsync(accountId, today, today.AddDays(daysAhead));

            var notifications = new List<UpcomingNotificationDto>();

            foreach (var credit in credits)
            {
                var nextDueDate = CalculateNextDueDate(credit);
                notifications.Add(new UpcomingNotificationDto(
                    CreditId: credit.CreditId,
                    ContractNumber: GenerateContractNumber(credit.CreditId),
                    DueDate: nextDueDate,
                    AmountDue: CalculateDueAmount(credit),
                    IsOverdue: nextDueDate < today,
                    ClientName: await GetClientNameAsync(credit.AccountId)
                ));
            }

            return notifications;
        }




        public async Task<bool> ResendNotificationAsync(int creditId)
        {
            var credit = await _creditRepository.GetCreditByIdAsync(creditId);
            if (credit == null) return false;

            await ProcessCreditNotification(credit, DateOnly.FromDateTime(DateTime.Today));
            return true;
        }

        private async Task<string> GetClientNameAsync(int accountId)
        {
            var userInfo = await _userServiceClient.GetUserInfoAsync(accountId);
            return userInfo != null ? $"{userInfo.FirstName} {userInfo.LastName}" : "Unknown";
        }



        private async Task SaveNotification(int accountId, string message, string status, DateTime? notificationDate)
        {
            var notification = new Notification
            {
                AccountId = accountId,
                Message = message,
                Status = status,
                NotificationDate = notificationDate ?? DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);

            // 💾 Ensure changes are saved
            await _notificationRepository.SaveChangesAsync();

            _logger.LogInformation($"Notification sauvegardée : {message}");
        }





    }
}
