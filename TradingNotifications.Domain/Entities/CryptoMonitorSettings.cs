namespace TradingNotifications.Domain.Entities;

public class CryptoMonitorSettings
{
    public List<string> CryptoList { get; set; }
    public int SmaPeriod { get; set; }
    public int RsiPeriod { get; set; }
    public int RsiBuyThreshold { get; set; }
}