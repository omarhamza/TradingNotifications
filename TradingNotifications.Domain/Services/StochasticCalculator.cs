using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingNotifications.Domain.Services
{
    public static class StochasticCalculator
    {
        public static (decimal percentK, decimal percentD) GetIndicators(
            List<decimal> closes,
            List<decimal> highs,
            List<decimal> lows,
            int period = 14,
            int dPeriod = 3)
        {
            if (closes.Count < period + dPeriod || highs.Count < period + dPeriod || lows.Count < period + dPeriod)
                return (0, 0);

            if (!(closes.Count == highs.Count && closes.Count == lows.Count))
                return (0, 0);

            List<decimal> kValues = new();

            for (int i = closes.Count - (period + dPeriod); i < closes.Count - period + 1; i++)
            {
                var highPeriod = highs.Skip(i).Take(period).ToList();
                var lowPeriod = lows.Skip(i).Take(period).ToList();

                decimal highestHigh = highPeriod.Max();
                decimal lowestLow = lowPeriod.Min();
                decimal close = closes[i + period - 1];

                decimal k = 100 * (close - lowestLow) / (highestHigh - lowestLow);
                kValues.Add(k);
            }

            // %K = dernière valeur calculée
            decimal percentK = kValues.Last();

            // %D = moyenne des derniers %K (souvent 3 périodes)
            decimal percentD = kValues.TakeLast(dPeriod).Average();

            return (Math.Round(percentK, 2), Math.Round(percentD, 2));
        }
    }
}