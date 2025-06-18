using System.Net.Http;
using TradingNotifications.Application;
using TradingNotifications.Domain.Entities;

namespace TradingNotifications.Infrastructure;

public class TelegramNotificationService : INotificationService
{
    private readonly string _botToken;
    private readonly string _chatId;
    private readonly HttpClient _httpClient;
    public TelegramNotificationService(string botToken, string chatId, HttpClient httpClient)
    {
        _botToken = botToken;
        _chatId = chatId;
        _httpClient = httpClient;
    }

    public async Task SendNotificationAsync(Notification notification)
    {
        var url = $"https://api.telegram.org/bot{_botToken}/sendMessage?chat_id={_chatId}&text={Uri.EscapeDataString(notification.Message)}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
    }
}