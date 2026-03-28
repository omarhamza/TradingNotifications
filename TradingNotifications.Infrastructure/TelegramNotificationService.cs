using System.Net.Http;
using TradingNotifications.Application;
using TradingNotifications.Domain.Entities;

namespace TradingNotifications.Infrastructure;

public class TelegramNotificationService : INotificationService
{
    private readonly string _botToken;
    private readonly string _chatId;
    private readonly IHttpClientFactory _httpClientFactory;
    public TelegramNotificationService(string botToken, string chatId, IHttpClientFactory httpClient)
    {
        _botToken = botToken;
        _chatId = chatId;
        _httpClientFactory = httpClient;
    }

    public async Task SendNotificationAsync(Notification notification)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var url = $"https://api.telegram.org/bot{_botToken}/sendMessage?chat_id={_chatId}&text={Uri.EscapeDataString(notification.Message)}";

        var response = await httpClient.GetStringAsync(url);
    }
}