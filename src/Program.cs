using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
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
                await threeFunds["Vanguard ETFs"].WritePricesToCsvPerYear(2012);
                await threeFunds["Vanguard Mutual Funds"].WritePricesToCsvPerYear(2011);

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

            string apiKey = File.ReadAllText("c:\\temp\\alphaVantageApiKey.txt");

            using (HttpClient client = new HttpClient())
            {
                await FetchQuote(apiKey, client, "vti");
                await FetchQuote(apiKey, client, "vtsax");
                await FetchQuote(apiKey, client, "msft");
            }
        }

        private static async Task FetchQuote(string apiKey, HttpClient client, string symbol)
        {
            Console.Write($"Fetching [{symbol}] {DateTime.Now.ToLongTimeString()}:  ");

            string QUERY_URL = "https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol="+symbol+"&apikey=" + apiKey;
            Uri queryUri = new Uri(QUERY_URL);
            
            string result = await client.GetStringAsync(queryUri);

            var json_root = JsonDocument.Parse(result).RootElement;
            JsonElement metadata, lastRefreshed;
            bool foundMetadata = json_root.TryGetProperty("Meta Data", out metadata);
            if (foundMetadata)
            {
                bool foundLastRefreshed = metadata.TryGetProperty("3. Last Refreshed", out lastRefreshed);
                if (foundLastRefreshed)
                {
                    string lastRefreshedString = lastRefreshed.GetString();
                    Console.Write(lastRefreshedString);

                    string fileName = symbol + "-" + lastRefreshedString.Replace(':','-') + ".json";
                    if (!File.Exists(fileName))
                    {
                        File.WriteAllText(fileName, result);
                    }

                    DateTime lastRefreshedDateTime = DateTime.Parse(lastRefreshedString);
                    JsonElement dayData = json_root.GetProperty("Time Series (Daily)").GetProperty(lastRefreshedDateTime.ToString("yyyy-MM-dd"));
                    string close = dayData.GetProperty("4. close").GetString();
                    string dividend = dayData.GetProperty("7. dividend amount").GetString();
                    Console.WriteLine($"  Price: {close} {(dividend != "0.0000" ? "Dividend: " + dividend : "")}");
                }
            }
        }
    }
}
