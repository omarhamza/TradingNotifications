namespace TradingNotifications.Domain.Services;

public static class RsiCalculator
{
    // 📈 RSI (Relative Strength Index)
    public static decimal GetWilderRSI(List<decimal> closes, int period) =>
        GetWilderRSIOverTime(closes, period).LastOrDefault();

    public static List<decimal> GetWilderRSIOverTime(List<decimal> closes, int period)
    {
        var rsiValues = new List<decimal>();

        if (closes.Count < period + 1)
            return rsiValues; // pas assez de données

        decimal avgGain = 0, avgLoss = 0;

        // 1. Moyenne initiale sur les gains et pertes
        for (int i = 1; i <= period; i++)
        {
            var delta = closes[i] - closes[i - 1];
            if (delta > 0) avgGain += delta;
            else avgLoss -= delta;
        }

        avgGain /= period;
        avgLoss /= period;

        // Calcul RSI initial
        decimal rs = avgLoss == 0 ? 0 : avgGain / avgLoss;
        rsiValues.Add(avgLoss == 0 ? 100 : 100 - (100 / (1 + rs)));

        // 2. Calcul lissé RSI pour chaque nouvelle valeur
        for (int i = period + 1; i < closes.Count; i++)
        {
            var delta = closes[i] - closes[i - 1];
            decimal gain = delta > 0 ? delta : 0;
            decimal loss = delta < 0 ? -delta : 0;

            avgGain = ((avgGain * (period - 1)) + gain) / period;
            avgLoss = ((avgLoss * (period - 1)) + loss) / period;

            rs = avgLoss == 0 ? 0 : avgGain / avgLoss;
            var rsi = avgLoss == 0 ? 100 : 100 - (100 / (1 + rs));
            rsiValues.Add(rsi);
        }

        return rsiValues;
    }
}