using daily.DataProviders;
using System;
using System.Collections.Generic;
using System.Net.Http;
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

                Console.WriteLine("Collecting prices:");
                threeFunds["Vanguard ETFs"].LoadPricesIntoFunds(2012);
                threeFunds["Vanguard Mutual Funds"].LoadPricesIntoFunds(2011);

                Console.WriteLine("Calculating perf:");
                await threeFunds["Vanguard ETFs"].OutputThreeFundPerfSummary(2012);
                await threeFunds["Vanguard Mutual Funds"].OutputThreeFundPerfSummary(2011);

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

            using (HttpClient client = new HttpClient())
            {
                await AlphaVantage.FetchQuote(client, "vti", TimeSeries.Monthly);
                await AlphaVantage.FetchQuote(client, "vxus", TimeSeries.Monthly);
                await AlphaVantage.FetchQuote(client, "bnd", TimeSeries.Monthly);
            }
        }
    }
}
