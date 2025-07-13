using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingNotifications.Domain.Services
{
    public static class StochasticCalculator
    {
        public static (decimal percentK, decimal percentD) GetIndicators(List<decimal> closes, int period = 14, int smoothing = 3)
        {
            if (closes.Count < period + smoothing) return (0, 0);

            List<decimal> percentKValues = new List<decimal>();

            for (int i = closes.Count - period - smoothing + 1; i <= closes.Count - period; i++)
            {
                var window = closes.Skip(i).Take(period).ToList();
                decimal highestHigh = window.Max();
                decimal lowestLow = window.Min();
                decimal currentClose = closes[i + period - 1];

                decimal percentK = (currentClose - lowestLow) / (highestHigh - lowestLow) * 100;
                percentKValues.Add(percentK);
            }

            decimal percentKLatest = percentKValues.Last();
            decimal percentD = percentKValues.TakeLast(smoothing).Average();

            return (percentKLatest, percentD);
        }
    }
}