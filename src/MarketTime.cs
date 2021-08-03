using System;

namespace daily
{
    public enum MarketTime
    {
        None = 0,
        Open,
        MutualFundPricesPublished,
        VanguardHistoricalPricesUpdated,
        MarketClosedAllDay,
        MarketClosed,
    }

    public static class MarketTimes
    {
        public static MarketTime GetMarketTime()
        {
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone);

            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                return MarketTime.MarketClosedAllDay;
            }

            double time = now.Hour + now.Minute / 60.0;

            if (time >= 9.5 && time <= 16.0)
            {
                return MarketTime.Open;
            }
            else if (time >= 18.0 && time < 18.125)
            {
                return MarketTime.MutualFundPricesPublished;
            }
            else if (time >= 18.75 || time < 9.5)
            {
                return MarketTime.VanguardHistoricalPricesUpdated;
            }
            else
            {
                return MarketTime.MarketClosedAllDay;
            }
        }
    }
}
