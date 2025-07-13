using System.Globalization;
using System.Text.Json;
using TradingNotifications.Domain.Entities;
using TradingNotifications.Domain.Services;

namespace TradingNotifications.Application;

public class CryptoAnalysisService : ICryptoAnalysisService
{
    private readonly INotificationService _notificationService;
    private readonly IHttpClientFactory _httpClientFactory;

    public CryptoAnalysisService(INotificationService notificationService, IHttpClientFactory httpClientFactory)
    {
        _notificationService = notificationService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task ProcessNotificationsAsync(IEnumerable<string> cryptoList, CryptoMonitorSettings settings)
    {

        if (settings == null || settings.CryptoList == null || !settings.CryptoList.Any())
        {
            Console.WriteLine("Aucune crypto-monnaie à surveiller.");
            return;
        }

        foreach (var symbol in cryptoList)
        {
            try
            {
                var candles = await GetHistoricalPricesAsync(symbol);

                if (candles == null || !candles.Any())
                {
                    Console.WriteLine($"Aucune donnée historique pour {symbol}.");
                    continue;
                }

                var message = string.Empty;
                if (ShouldIBuyCrypto(candles.Select(c => c.Close).ToList(), out message))
                {
                    Console.WriteLine($"ACHETER {symbol} - Prix actuel: {candles.Last().Close:F2}");
                    await _notificationService.SendNotificationAsync(new Notification($"ACHETER {symbol} - Prix actuel: {candles.Last().Close:F2} \n {message}"));
                }
                else if (ShouldISellCrypto(candles.Select(c => c.Close).ToList(), out message))
                {
                    Console.WriteLine($"VENDRE {symbol} - Prix actuel: {candles.Last().Close:F2}");
                    await _notificationService.SendNotificationAsync(new Notification($"VENDRE {symbol} - Prix actuel: {candles.Last().Close:F2} \n {message}"));
                }

                Console.WriteLine($"{symbol} surveillé - Prix actuel: {candles.Last().Close:F2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur avec {symbol}: {ex.Message}");
            }
        }
    }

    private async Task<List<Candle>> GetHistoricalPricesAsync(string symbol, string interval = "1h", int limit = 100)
    {
        var url = $"https://api.binance.com/api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}";
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetStringAsync(url);
        var raw = JsonSerializer.Deserialize<List<List<JsonElement>>>(response);

        var candles = new List<Candle>();
        foreach (var item in raw)
        {
            candles.Add(new Candle
            {
                OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(item[0].GetInt64()).DateTime,
                Open = decimal.Parse(item[1].GetString(), CultureInfo.InvariantCulture),
                High = decimal.Parse(item[2].GetString(), CultureInfo.InvariantCulture),
                Low = decimal.Parse(item[3].GetString(), CultureInfo.InvariantCulture),
                Close = decimal.Parse(item[4].GetString(), CultureInfo.InvariantCulture),
                Volume = decimal.Parse(item[5].GetString(), CultureInfo.InvariantCulture),
            });
        }

        return candles;
    }

    private bool ShouldIBuyCrypto(List<decimal> closingPrices, out string message)
    {
        message = string.Empty;
        int period = 14;

        // 1. 📈 Détection d'une poussée du RSI (hausse brutale > 10 pts en 15 minutes)
        bool isSurge = Algorithms101.IsRSISurge(closingPrices, period, out string surgeMsg);

        if (isSurge)
        {
            message = "🚀 RSI Surge détecté : " + surgeMsg;
            return true;
        }

        // 2. 🧾 Moyenne mobile simple (SMA)
        var sma = SmaCalculator.GetIndicator(closingPrices, period);

        // 3. 📈 RSI (Wilder)
        var rsi = RsiCalculator.GetWilderRSI(closingPrices, period);

        var currentPrice = closingPrices.Last();
        var previousPrice = closingPrices[closingPrices.Count - 2];

        // 4. 📉 MACD (Moving Average Convergence Divergence)
        // détecter les croisements après plusieurs bougies
        var macd = MacdCalculator.GetIndicator(closingPrices);

        // 5. 📈 Slope de la Moyenne Mobile Simple (SMA)
        var smaSlope = SmaCalculator.GetSmaSlope(closingPrices, period);

        // 6. 📊 Conditions d'achat
        bool shouldIBuy =
                macd.Histogram > 0 && // Croisement MACD au-dessus du Signal => signal d’achat
                macd.Macd > macd.Signal &&
                rsi > 20 && rsi < 80 &&
                currentPrice > sma &&
                currentPrice > previousPrice &&
                smaSlope > 0; // SMA en pente positive

        if (shouldIBuy)
        {
            message = $"🔍 Conditions d'achat remplies : RSI={rsi:F2}, Prix actuel={currentPrice:F2} > SMA={sma:F2}";
        }

        return shouldIBuy;
    }

    private bool ShouldISellCrypto(List<decimal> closingPrices, out string message)
    {
        // Paramètres
        int period = 14;
        message = string.Empty;

        // 1. 📈 RSI
        var rsi = RsiCalculator.GetWilderRSI(closingPrices, period);

        // 2. 📊 Vérification de la tendance baissière
        if (closingPrices.Count < 2)
        {
            message = "Pas assez de données pour évaluer la tendance.";
            return false;
        }

        if (rsi > 80) // Surachat
        {
            // Dernier prix et prix précédent
            {
                var lastPrice = closingPrices.Last();
                var previousPrice = closingPrices[closingPrices.Count - 2];

                message = $"🔺RSI = {rsi:F2} > 80 \n 🔴Signal de VENTE immédiat\n Current price: {lastPrice}\n Previous price: {previousPrice}";
                return lastPrice < previousPrice; // Vente en cas de tendance baissière des prix.
            }
        }

        return false;
    }
}