using System.Globalization;
using System.Text.Json;
using TradingNotifications.Domain.Entities;

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

                if (ShouldBuyCrypto(candles.Select(c => c.Close).ToList()))
                {
                    Console.WriteLine($"ACHETER {symbol} - Prix actuel: {candles.Last().Close:F2}");
                    await _notificationService.SendNotificationAsync(new Notification($"ACHETER {symbol} - Prix actuel: {candles.Last().Close:F2}"));
                }
                else
                {
                    Console.WriteLine($"{symbol} surveillé - Prix actuel: {candles.Last().Close:F2}");
                }
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

    private bool ShouldBuyCrypto(List<decimal> closingPrices)
    {
        // Paramètres
        int period = 14;
        decimal currentPrice = closingPrices.Last();

        // 0. 📈 Is RSI Surge
        if (Algorithms101.IsRSISurge(closingPrices, period))
        {
            return true;
        }

        // 1. 🧾 SMA
        var sma = Algorithms101.SimpleMovingAverage(closingPrices, period);

        // 2. 📈 RSI
        var rsi = Algorithms101.CalculateRSI(closingPrices, period);

        // 3. 🔍 LinearSearch pour savoir si la valeur actuelle existe dans l'historique (exemple ludique)
        int index = Algorithms101.LinearSearch(closingPrices.Select(c => (int)c).ToArray(), (int)currentPrice);
        bool isCurrentInHistory = index != -1;

        // 4. 🔍 BinarySearch (sur données triées) - teste si SMA apparaît dans l'historique (arrondi)
        var sorted = closingPrices.Select(c => (int)c).OrderBy(x => x).ToArray();
        int smaSearch = Algorithms101.BinarySearch(sorted, (int)sma);

        // 5. 🔄 BubbleSort (expérimental - juste pour appel de méthode)
        var testArray = new int[] { 5, 3, 1, 4, 2 };
        Algorithms101.BubbleSort(testArray);

        // 6. 🧮 Factorielle (debug ou expérimental, ex: "combien de scénarios ?" → n!)
        var factorial = Algorithms101.Factorial(5);

        // 7. 🧠 IsPalindrome - ludique : on vérifie si la représentation textuelle du prix est symétrique
        bool isPalindromePrice = Algorithms101.IsPalindrome(currentPrice.ToString("F2").Replace(".", ""));

        // 8. 🧩 AreAnagrams - exemple symbolique : anagramme entre deux prix récents
        bool areAnagrams = Algorithms101.AreAnagrams(
            closingPrices[^1].ToString("F0"),
            closingPrices[^2].ToString("F0"));

        // 🧠 Logique de décision personnalisée
        bool shouldBuy = rsi < 30 && currentPrice < sma && isCurrentInHistory && smaSearch != -1;

        Console.WriteLine($"SMA: {sma}, RSI: {rsi}, Palindrome? {isPalindromePrice}, Anagramme? {areAnagrams}");

        return shouldBuy;
    }
}