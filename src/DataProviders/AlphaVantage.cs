using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace daily.DataProviders
{
    public class AlphaVantage
    {
        public TimeSeries TimeSeries { get; private set; }
        public string Symbol { get; private set; }
        public string LastRefreshed { get; private set; }


        private bool loaded = false;
        private JsonElement json_root;

        public AlphaVantage(string symbol, TimeSeries timeSeries)
        {
            Symbol = symbol;
            TimeSeries = timeSeries;
        }

        private async Task FetchData()
        {
            FileInfo montlyPriceFile = new FileInfo($"prices\\{Symbol}\\alphaVantageMonthly-{Symbol}.json");
            string jsonData;
            if (!montlyPriceFile.Exists)
            {
                var client = new HttpClient();
                string apiKey = File.ReadAllText("c:\\temp\\alphaVantageApiKey.txt");

                string function = "TIME_SERIES_";
                switch (TimeSeries)
                {
                    case TimeSeries.Daily:
                        function += "DAILY_ADJUSTED";
                        break;
                    case TimeSeries.Monthly:
                        function += "MONTHLY_ADJUSTED";
                        break;
                    default:
                        throw new ArgumentException("timeSeries");
                }

                string QUERY_URL = $"https://www.alphavantage.co/query?function={function}&symbol={Symbol}&apikey={apiKey}";
                jsonData = await client.GetStringAsync(new Uri(QUERY_URL));

                try
                {
                    json_root = JsonDocument.Parse(jsonData).RootElement;
                    LastRefreshed = GetLastRefreshed(json_root);
                    await File.WriteAllTextAsync(montlyPriceFile.FullName, jsonData);
                    loaded = true;
                }
                catch (Exception)
                {
                    LastRefreshed = "error: API Limit Reached"; // likely due to API calls per minute restrictions
                }
            }
            else
            {
                jsonData = await File.ReadAllTextAsync(montlyPriceFile.FullName);
                json_root = JsonDocument.Parse(jsonData).RootElement;
            }
        }

        private static string GetLastRefreshed(JsonElement json_root)
        {
            JsonElement metadata, lastRefreshed;
            bool foundMetadata = json_root.TryGetProperty("Meta Data", out metadata);
            if (foundMetadata)
            {
                bool foundLastRefreshed = metadata.TryGetProperty("3. Last Refreshed", out lastRefreshed);
                if (foundLastRefreshed)
                {
                    return lastRefreshed.GetString();
                }
            }

            throw new InvalidDataException();
        }

        public async Task<JsonElement> GetDataRoot()
        {
            if (!loaded)
            {
                await FetchData();
            }

            string dataRootPropetyName;
            switch (TimeSeries)
            {
                case TimeSeries.Daily:
                    dataRootPropetyName = "Time Series (Daily)";
                    break;
                case TimeSeries.Monthly:
                    dataRootPropetyName = "Monthly Adjusted Time Series";
                    break;
                default:
                    throw new ArgumentException("timeSeries");
            }

            return json_root.GetProperty(dataRootPropetyName);
        }

        public async Task FetchQuote()
        {
            if (!loaded)
            {
                await FetchData();
            }

            string lastRefreshedString = GetLastRefreshed(json_root);
            DateTime lastRefreshedDateTime = DateTime.Parse(lastRefreshedString);
            string dateKey = lastRefreshedDateTime.ToString("yyyy-MM-dd");
            JsonElement dataRoot = await GetDataRoot();
            JsonElement dayData = dataRoot.GetProperty(dateKey);
            string close = dayData.GetProperty("4. close").GetString();
            string dividend = dayData.GetProperty("7. dividend amount").GetString();
            Console.WriteLine($"  Price: {close} {(dividend != "0.0000" ? "Dividend: " + dividend : "")}");
        }
    }
}
