using TradingNotifications.Domain.Entities;

namespace TradingNotifications.Application
{
    public interface ICryptoAnalysisService
    {
        public Task ProcessNotificationsAsync(
            IEnumerable<string> cryptoList,
            CryptoMonitorSettings settings
            );
    }
}