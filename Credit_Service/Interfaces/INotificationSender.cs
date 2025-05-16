using Credit_Service.DTOs;

namespace Credit_Service.Interfaces
{
    public interface INotificationSender
    {
        Task SendAsync(CreditNotification notification);
    }
}
