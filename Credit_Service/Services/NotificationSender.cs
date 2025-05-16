// NotificationSender.cs
using Credit_Service.DTOs;
using Credit_Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace Credit_Service.Services
{
    public class NotificationSender : INotificationSender
    {
        private readonly ILogger<NotificationSender> _logger;

        public NotificationSender(ILogger<NotificationSender> logger)
        {
            _logger = logger;
        }

        public async Task SendAsync(CreditNotification notification)
        {
            // Implementation here
            _logger.LogInformation($"Sending notification to {notification.ClientEmail}");
            await Task.CompletedTask;
        }
    }
}