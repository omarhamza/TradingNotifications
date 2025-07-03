using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingNotifications.Application;
using TradingNotifications.Infrastructure;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var botToken = Environment.GetEnvironmentVariable("TelegramBotToken");
var chatId = Environment.GetEnvironmentVariable("TelegramChatId");

builder.Services.AddSingleton<INotificationService>(serviceProvider =>
{
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    return new TelegramNotificationService(botToken!, chatId!, httpClientFactory);
});
builder.Services.AddSingleton<ICryptoAnalysisService, CryptoAnalysisService>();

builder.Services.AddHttpClient();
builder.Services.AddLogging();

builder.Build().Run();