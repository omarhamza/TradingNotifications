using TradingNotifications.Domain.Entities;

namespace TradingNotifications.Domain.Services;

public static class MacdCalculator
{
    public static MacdResult GetIndicator(List<decimal> closes) => CalculateMacd(closes).LastOrDefault();

    private static List<MacdResult> CalculateMacd(List<decimal> closes, int shortPeriod = 12, int longPeriod = 26, int signalPeriod = 9)
    {
        if (closes.Count < longPeriod + signalPeriod)
            throw new ArgumentException("Pas assez de données pour calculer le MACD");

        var emaShort = CalculateEMA(closes, shortPeriod);
        var emaLong = CalculateEMA(closes, longPeriod);

        // MACD = EMA(short) - EMA(long)
        var macdLine = new List<decimal>();
        for (int i = 0; i < closes.Count; i++)
        {
            if (i < longPeriod - 1)
                macdLine.Add(0); // pas assez de données
            else
                macdLine.Add(emaShort[i] - emaLong[i]);
        }

        // Signal line = EMA du MACD
        var signalLine = CalculateEMA(macdLine, signalPeriod);

        // Histogram = MACD - Signal
        var result = new List<MacdResult>();
        for (int i = 0; i < closes.Count; i++)
        {
            result.Add(new MacdResult(Macd: macdLine[i], Signal: signalLine[i], Histogram: macdLine[i] - signalLine[i]));
        }

        return result;
    }

    // EMA = Exponential Moving Average
    private static List<decimal> CalculateEMA(List<decimal> values, int period)
    {
        var ema = new List<decimal>();
        decimal multiplier = 2m / (period + 1);
        decimal? previousEma = null;

        for (int i = 0; i < values.Count; i++)
        {
            if (i < period - 1)
            {
                ema.Add(0);
                continue;
            }

            if (i == period - 1)
            {
                decimal sma = values.Skip(i - period + 1).Take(period).Average();
                ema.Add(sma);
                previousEma = sma;
            }
            else
            {
                decimal currentEma = ((values[i] - previousEma.Value) * multiplier) + previousEma.Value;
                ema.Add(currentEma);
                previousEma = currentEma;
            }
        }

        return ema;
    }
}