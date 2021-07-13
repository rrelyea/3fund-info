using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace daily.DataProviders
{
    public static class AlphaVantage
    {
        public static async Task FetchQuote(HttpClient client, string symbol, TimeSeries timeSeries)
        {

            string apiKey = File.ReadAllText("c:\\temp\\alphaVantageApiKey.txt");

            Console.Write($"Fetching [{symbol}] {DateTime.Now.ToLongTimeString()}:  ");

            string function = "TIME_SERIES_";
            string dataRootPropetyName;
            switch (timeSeries)
            {
                case TimeSeries.Daily:
                    function += "DAILY_ADJUSTED";
                    dataRootPropetyName = "Time Series (Daily)";
                    break;
                case TimeSeries.Monthly:
                    function += "MONTHLY_ADJUSTED";
                    dataRootPropetyName = "Monthly Adjusted Time Series";
                    break;
                default:
                    throw new ArgumentException("timeSeries");
            }

            string QUERY_URL = $"https://www.alphavantage.co/query?function={function}&symbol={symbol}&apikey={apiKey}";
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

                    string fileName = symbol + "-" + lastRefreshedString.Replace(':', '-') + ".json";
                    if (!File.Exists(fileName))
                    {
                        File.WriteAllText(fileName, result);
                    }

                    DateTime lastRefreshedDateTime = DateTime.Parse(lastRefreshedString);
                    string dateKey = lastRefreshedDateTime.ToString("yyyy-MM-dd");
                    JsonElement dayData = json_root.GetProperty(dataRootPropetyName).GetProperty(dateKey);
                    string close = dayData.GetProperty("4. close").GetString();
                    string dividend = dayData.GetProperty("7. dividend amount").GetString();
                    Console.WriteLine($"  Price: {close} {(dividend != "0.0000" ? "Dividend: " + dividend : "")}");
                }
            }
        }
    }
}
