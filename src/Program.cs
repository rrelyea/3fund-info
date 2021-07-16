﻿using daily.DataProviders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace daily
{
    class Program
    {
        static Timer timer;
        static async Task Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "/watch")
            {
                Console.WriteLine("Watching for latest prices:");
                double minutes = .1;
                timer = new Timer(minutes * 60 * 1000);
                timer.Elapsed += WatchTimer_Elapsed;
                timer.Enabled = true;
                timer.Start();

                while (true) ;
            }
            else
            {
                Dictionary<string, ThreeFund> threeFunds = InitializeThreeFunds();
                MarketTime marketTime = GetMarketTime();

                await threeFunds["Vanguard ETFs"].CreatePerfSummary(2012, marketTime);
                await threeFunds["Vanguard Mutual Funds"].CreatePerfSummary(2011, marketTime);
            }
        }

        private static MarketTime GetMarketTime()
        {
            DateTime now = DateTime.UtcNow;
            double time = (now.Hour - 4.0) + now.Minute / 60.0;

            if (time >= 9.5 && time < 16.1)
            {
                return MarketTime.Open;
            }
            else if (time > 18.0 && time < 18.0)
            {
                return MarketTime.MutualFundPricesPublished;
            }
            else if ((time > 18.75 && time < 24.00) || (time > -6.00 && time < 0))
            {
                return MarketTime.VanguardHistoricalPricesUpdated;
            }
            else
            {
                return MarketTime.None;
            }
        }

        private static Dictionary<string, ThreeFund> InitializeThreeFunds()
        {
            Dictionary<string, ThreeFund> threeFunds = new Dictionary<string, ThreeFund>();

            threeFunds.Add("Vanguard Mutual Funds", new ThreeFund("vtsax", "vtiax", "vbtlx", FundStyle.MutualFund, "Vanguard"));
            threeFunds.Add("Vanguard ETFs", new ThreeFund("vti", "vxus", "bnd", FundStyle.ETF, "Vanguard"));

            return threeFunds;
        }

        private static async void WatchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Interval = 5 * 60 * 1000;

            await AlphaVantage.FetchQuote("vti", TimeSeries.Monthly);
            await AlphaVantage.FetchQuote("vxus", TimeSeries.Monthly);
            await AlphaVantage.FetchQuote("bnd", TimeSeries.Monthly);
        }
    }
}
