using System.Net.Http;
using TradingNotifications.Application;
using TradingNotifications.Domain.Entities;

namespace TradingNotifications.Infrastructure;

public class TelegramNotificationService : INotificationService
{
    private readonly string _botToken;
    private readonly string _chatId;

    public TelegramNotificationService(string botToken, string chatId)
    {
        _botToken = botToken;
        _chatId = chatId;
    }

    public async Task SendNotificationAsync(Notification notification)
    {
        var url = $"https://api.telegram.org/bot{_botToken}/sendMessage?chat_id={_chatId}&text={Uri.EscapeDataString(notification.Message)}";

        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
    }
}
