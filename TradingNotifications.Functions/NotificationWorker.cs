using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TradingNotifications.Functions;

public class NotificationWorker
{
    private readonly ILogger _logger;

    public NotificationWorker(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<NotificationWorker>();
    }

    /// <summary>
    /// Run the worker each 15 min
    /// </summary>
    /// <param name="myTimer"></param>
    [Function("NotificationWorker")]
    public void Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.UtcNow);
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}