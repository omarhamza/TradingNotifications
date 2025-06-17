using TradingNotifications.Domain.Entities;

namespace TradingNotifications.Application;

public interface INotificationService
{
    Task SendNotificationAsync(Notification notification);
}