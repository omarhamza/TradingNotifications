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
                var message = string.Empty;
                decimal currentPrice = 0;
                var oneHourCandles = Task.Run(async () =>
                {
                    var candles = await GetHistoricalPricesAsync(symbol, "1h");
                    var prediction = DecideTradeAction(candles.Take(candles.Count - 1).ToList());
                    return prediction;
                });

                var fourHoursCandles = Task.Run(async () =>
                {
                    var candles = await GetHistoricalPricesAsync(symbol, "4h");
                    currentPrice = candles.Last().Close;
                    var prediction = DecideTradeAction(candles.Take(candles.Count - 1).ToList());
                    return prediction;
                });

                // Run tasks in parallel  
                await Task.WhenAll(oneHourCandles, fourHoursCandles);

                var fifteenMinDecision = fourHoursCandles.Result;
                var oneHourDecision = oneHourCandles.Result;

                if (fifteenMinDecision.Decision == Decision.Buy && oneHourDecision.Decision == Decision.Buy)
                {
                    Console.WriteLine($"ACHETER {symbol} - Prix actuel: {currentPrice:F2}");
                    await _notificationService.SendNotificationAsync(new Notification($"ACHETER {symbol} - Prix actuel: {currentPrice:F2} \n {message}"));
                }
                else if(fifteenMinDecision.Decision == Decision.Sell || oneHourDecision.Decision == Decision.Sell)
                {
                    Console.WriteLine($"VENDRE {symbol} - Prix actuel: ");
                    await _notificationService.SendNotificationAsync(new Notification($"VENDRE {symbol} - Prix actuel: {currentPrice:F2} \n {message}"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur avec {symbol}: {ex.Message}");
            }
        }
    }

    private Result DecideTradeAction(List<Candle> candles)
    {
        if (candles == null || !candles.Any())
        {
            return new Result(Decision.None, "Aucune donnée de bougie disponible pour l'analyse.");
        }

        var closes = candles.Select(c => c.Close).ToList();
        var highs = candles.Select(c => c.High).ToList();
        var lows = candles.Select(c => c.Low).ToList();

        return DecideTradeAction(closes, highs, lows);
    }

    private async Task<List<Candle>> GetHistoricalPricesAsync(string symbol, string interval, int limit = 100)
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

    private Result DecideTradeAction(List<decimal> closes, List<decimal> highs, List<decimal> lows)
    {
        int period = 14;

        // 📈 Détection d'une poussée du RSI (hausse brutale > 10 pts en 15 minutes)
        bool isSurge = Algorithms101.IsRSISurge(closes, period, out string surgeMsg);

        if (isSurge)
            return new Result(Decision.Buy, "🚀 RSI Surge détecté : " + surgeMsg);

        // Stochastic
        var (percentK, percentD) = StochasticCalculator.GetIndicators(closes, highs, lows);
        bool buySignal = percentK > percentD && percentK < 40;
        bool sellSignal = percentK < percentD && percentK > 65;

        if (buySignal)
            return new Result(Decision.Buy, $"🚀 Signal d'achat Stochastic : %K={percentK:F2}, %D={percentD:F2}");

        // 🧾 Moyenne mobile simple (SMA)
        var sma = SmaCalculator.GetIndicator(closes, period);

        // 📈 RSI (Wilder)
        var rsi = RsiCalculator.GetWilderRSI(closes, period);

        var currentPrice = closes.Last();
        var previousPrice = closes[closes.Count - 2];

        // 📉 MACD (Moving Average Convergence Divergence)
        // détecter les croisements après plusieurs bougies
        var macd = MacdCalculator.GetIndicator(closes);

        // 📈 Slope de la Moyenne Mobile Simple (SMA)
        var smaSlope = SmaCalculator.GetSmaSlope(closes, period);

        // 📊 Conditions d'achat
        bool shouldIBuy =
                macd.Histogram > 0 && // Croisement MACD au-dessus du Signal => signal d’achat
                macd.Macd > macd.Signal &&
                rsi > 30 && rsi < 70 &&
                currentPrice > sma &&
                currentPrice > previousPrice &&
                smaSlope > 0; // SMA en pente positive

        var shouldISell = (rsi > 80 || sellSignal) && // Surachat
                currentPrice < previousPrice; // Vente en cas de tendance baissière des prix.


        return shouldIBuy ?
                new Result(Decision.Buy, $"🔍 Conditions d'achat remplies : RSI={rsi:F2}, Prix actuel={currentPrice:F2} > SMA={sma:F2}") :
                shouldISell ?
                new Result(Decision.Sell, $"🔺RSI = {rsi:F2} > 80 \n Stochastic = {percentK:F2} > 80 \n 🔴Signal de VENTE immédiat\n Current price: {currentPrice:F2}\n Previous price: {previousPrice:F2}") :
                new Result(Decision.None, "Aucune condition d'achat ou de vente remplie.");
    }
}