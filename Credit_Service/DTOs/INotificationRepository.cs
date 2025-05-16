using Credit_Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Credit_Service.DTOs
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<bool> ExistsSimilarAsync(int accountId, string messagePrefix);
        Task SaveChangesAsync();

    }

    public class NotificationRepository : INotificationRepository
    {
        private readonly CreditDBcontext _context;
        private readonly ILogger<NotificationRepository> _logger;

        public NotificationRepository(CreditDBcontext context, ILogger<NotificationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(Notification notification)
        {
            try
            {
                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Added notification ID {notification.NotificationId} for account {notification.AccountId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding notification");
                throw; // Re-throw for controller error handling
            }
        }

        public async Task<bool> ExistsSimilarAsync(int accountId, string messagePrefix)
        {
            return await _context.Notifications
                .AsNoTracking()
                .AnyAsync(n => n.AccountId == accountId
                            && n.Message.StartsWith(messagePrefix));
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification successfully saved to the database.");

        }


    }
}
