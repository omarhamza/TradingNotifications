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

builder.Services.AddSingleton<INotificationService>(new TelegramNotificationService(botToken!, chatId!));
builder.Services.AddSingleton<ICryptoAnalysisService, CryptoAnalysisService>();

builder.Services.AddLogging();

builder.Build().Run();
