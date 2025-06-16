using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TradingNotifications.Application;
using TradingNotifications.Domain.Entities;

namespace TradingNotifications.Functions;

public class NotificationWorker
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly IConfiguration _configuration;
    private readonly INotificationService _notificationService;
    private readonly ICryptoAnalysisService _notificationProcessor;

    public NotificationWorker(ILoggerFactory loggerFactory,
        IConfiguration configuration, 
        INotificationService notificationService, 
        ICryptoAnalysisService notificationProcessor)
    {
        _logger = loggerFactory.CreateLogger<NotificationWorker>();
        _configuration = configuration;
        _notificationService = notificationService;
        _notificationProcessor = notificationProcessor;
    }

    /// <summary>
    /// Run the worker each 15 min
    /// </summary>
    /// <param name="myTimer"></param>
    [Function("NotificationWorker")]
    public void Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.UtcNow);

        var settings = _configuration.GetSection("CryptoMonitorSettings").Get<CryptoMonitorSettings>() ?? new CryptoMonitorSettings();

        _notificationProcessor
            .ProcessNotificationsAsync(settings.CryptoList, settings)
            .GetAwaiter()
            .GetResult();

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}