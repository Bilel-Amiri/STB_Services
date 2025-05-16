using Credit_Service.DTOs;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Credit_Service.Interfaces
{
    public interface INotificationService : IHostedService
    {
        Task<NotificationCheckResult> CheckDueDatesAndNotifyAsync();
        Task<IEnumerable<UpcomingNotificationDto>> GetUpcomingNotificationsAsync(int clientId, int daysAhead);
        Task<bool> ResendNotificationAsync(int creditId);
       
    }
   
}