using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace daily.DataProviders
{
    public static class AlphaVantage
    {
        public static async Task<JsonElement> FetchAllData(string symbol, TimeSeries timeSeries)
        {
            bool writeDebugJsonFile = false;
            var client = new HttpClient();
            string apiKey = File.ReadAllText("c:\\temp\\alphaVantageApiKey.txt");

            string function = "TIME_SERIES_";
            switch (timeSeries)
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

            string QUERY_URL = $"https://www.alphavantage.co/query?function={function}&symbol={symbol}&apikey={apiKey}";
            Uri queryUri = new Uri(QUERY_URL);

            string result = await client.GetStringAsync(queryUri);

            JsonElement json_root = JsonDocument.Parse(result).RootElement;
            string lastRefreshedString = null;

            try
            {
                lastRefreshedString = GetLastRefreshed(json_root);
            }
            catch (Exception)
            {
                lastRefreshedString = "bad Last Refreshed"; // likely due to API calls per minute restrictions
            }
            finally
            {
                string fileName = symbol + "-" + lastRefreshedString.Replace(':', '-') + ".json";
                if (writeDebugJsonFile)
                {
                    File.WriteAllText(fileName, result);
                }
            }

            return json_root;
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

        public static JsonElement GetDataRoot(JsonElement json_root, TimeSeries timeSeries)
        {
            string dataRootPropetyName;
            switch (timeSeries)
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

        public static async Task FetchQuote(string symbol, TimeSeries timeSeries)
        {
            JsonElement json_root = await FetchAllData(symbol, timeSeries);
            string lastRefreshedString = GetLastRefreshed(json_root);
            DateTime lastRefreshedDateTime = DateTime.Parse(lastRefreshedString);
            string dateKey = lastRefreshedDateTime.ToString("yyyy-MM-dd");
            JsonElement dataRoot = GetDataRoot(json_root, timeSeries);
            JsonElement dayData = dataRoot.GetProperty(dateKey);
            string close = dayData.GetProperty("4. close").GetString();
            string dividend = dayData.GetProperty("7. dividend amount").GetString();
            Console.WriteLine($"  Price: {close} {(dividend != "0.0000" ? "Dividend: " + dividend : "")}");
        }
    }
}
