using TradingNotifications.Domain.Services;

namespace TradingNotifications.Application;

public static class Algorithms101
{
    // 🧾 Moyenne Mobile Simple (SMA)
    public static decimal SimpleMovingAverage(List<decimal> values, int period)
    {
        if (values.Count < period) return 0;
        return values.Skip(values.Count - period).Take(period).Average();
    }

    // 📈 Moyenne Mobile Exponentielle (EMA)
    public static decimal ExponentialMovingAverage(List<decimal> values, int period)
    {
        if (values.Count < period) return 0;

        decimal k = 2m / (period + 1);
        decimal ema = values.Take(period).Average();

        for (int i = period; i < values.Count; i++)
        {
            ema = (values[i] - ema) * k + ema;
        }

        return ema;
    }

    // 📉 MACD (EMA 12 - EMA 26)
    public static (decimal macd, decimal signal, decimal histogram) CalculateMACD(List<decimal> values)
    {
        if (values.Count < 26) return (0, 0, 0);

        var ema12 = ExponentialMovingAverage(values, 12);
        var ema26 = ExponentialMovingAverage(values, 26);
        var macd = ema12 - ema26;

        var macdSeries = new List<decimal>();
        for (int i = 0; i < values.Count - 8; i++)
        {
            var subList = values.Skip(i).Take(9).ToList();
            var shortEma = ExponentialMovingAverage(subList, 12);
            var longEma = ExponentialMovingAverage(subList, 26);
            macdSeries.Add(shortEma - longEma);
        }
        var signal = ExponentialMovingAverage(macdSeries, 9);
        var histogram = macd - signal;
        return (macd, signal, histogram);
    }

    // 🚨 Détection de franchissement RSI de 30 à 40 en un intervalle (ex. 15min)
    public static bool IsRSISurge(List<decimal> closes, int period, out string message)
    {
        message = string.Empty;

        var rsiList = RsiCalculator.GetWilderRSIOverTime(closes, period);

        if (rsiList.Count < 2)
            throw new InvalidOperationException("Pas assez de données pour calculer deux valeurs RSI.");

        decimal latestRsi = rsiList.Last();
        decimal previousRsi = rsiList[rsiList.Count - 2]; // RSI 15 min avant

        decimal currentPrice = closes.Last();
        message = $"\U0001F4C8 RSI Surge detected for current price {currentPrice:F2}. Consider buying.\n Rsi before: {previousRsi:F2} \n rsi now: {latestRsi:F2}";

        return (latestRsi - previousRsi >= 10 && previousRsi <= 50) ||
               (previousRsi <= 30 && latestRsi >= 40);
    }
}