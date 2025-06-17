using TradingNotifications.Domain.Entities;

namespace TradingNotifications.Application;

public class SendNotification
{
    private readonly INotificationService _notificationService;

    public SendNotification(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task ExecuteAsync(string message)
    {
        var notification = new Notification(message);
        await _notificationService.SendNotificationAsync(notification);
    }
}