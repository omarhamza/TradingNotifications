using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingNotifications.Domain.Entities
{
    public record MacdResult(decimal Macd,
                             decimal Signal,
                             decimal Histogram); // MACD - Signal
}