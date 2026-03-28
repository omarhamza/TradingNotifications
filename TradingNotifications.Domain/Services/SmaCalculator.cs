namespace TradingNotifications.Domain.Services;

public static class SmaCalculator
{
    // 🧾 Moyenne Mobile Simple (SMA)
    public static decimal GetIndicator(List<decimal> values, int period)
    {
        if (values.Count < period) return 0;
        return values.Skip(values.Count - period).Take(period).Average();
    }

    // 📈 Slope de la Moyenne Mobile Simple (SMA)
    public static decimal GetSmaSlope(List<decimal> prices, int smaPeriod = 14, int slopePeriod = 3)
    {
        if (prices.Count < smaPeriod + slopePeriod) return 0;

        var smaValues = new List<decimal>();

        for (int i = prices.Count - smaPeriod - slopePeriod + 1; i <= prices.Count - smaPeriod; i++)
        {
            var window = prices.Skip(i).Take(smaPeriod).ToList();
            var sma = window.Average();
            smaValues.Add(sma);
        }

        // Slope = différence entre la dernière et la première SMA
        return smaValues.Last() - smaValues.First();
    }
}