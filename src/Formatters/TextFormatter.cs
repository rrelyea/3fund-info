using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace daily.Formatters
{
    public class TextFormatter
    {
        public static async Task OutputTextFile(ThreeFund threeFund, int startYear, PerfCalculator perfCalc, int stock, int intl, int bond, Dictionary<string, FundValue> perfSummaries)
        {
            FileInfo outputFile = new FileInfo($"perf\\{stock}-{bond}\\{stock}-{bond} ({intl}% intl)-{threeFund.StockFund.UpperSymbol}-{threeFund.BondFund.UpperSymbol}-{threeFund.InternationStockFund.UpperSymbol}.txt");
            if (!outputFile.Directory.Exists)
            {
                outputFile.Directory.Create();
            }

            StringBuilder summarySB = new StringBuilder();
            summarySB.AppendLine($"Performance for {stock}/{bond} ({intl}% intl)  {threeFund.StockFund.UpperSymbol}/{threeFund.BondFund.UpperSymbol} ({threeFund.InternationStockFund.UpperSymbol})");
            summarySB.AppendLine();
            summarySB.AppendLine("  Appreciation % |  Dividend %");
            summarySB.AppendLine(CreateTextPerfSummary(perfSummaries));
            await File.WriteAllTextAsync(outputFile.FullName, summarySB.ToString());
            Console.Write(".");
        }

        private static string CreateTextPerfSummary(Dictionary<string, FundValue> perfSummaries)
        {
            var summarySB = new StringBuilder();
            string year = null;
            string currentYear = DateTime.Now.Year.ToString();
            bool daysHeaderShown = false;
            foreach (var date in perfSummaries.Keys)
            {
                string[] chunks = date.Split('-');
                if (chunks[0] != year)
                {
                    year = chunks[0];
                    summarySB.AppendLine();
                    summarySB.AppendLine("----------------------------------");
                    summarySB.AppendLine($"{year}:");
                }
                FundValue summaryData = perfSummaries[date];
                if (chunks.Length == 2)
                {
                    if (date == DateTime.Now.ToString("yyyy-MMM"))
                    {
                        summarySB.AppendLine("--------------------");
                    }
                    summarySB.AppendLine($"    {chunks[1]} {summaryData.Value,7: ##.00;-##.00}%         {summaryData.Dividend: ##.00}%");
                }
                else if (chunks.Length == 1)
                {
                    summarySB.AppendLine("==================================");
                    string ytdStr = year == currentYear ? "YTD " : "Year";
                    summarySB.AppendLine($"{chunks[0]} {ytdStr}{summaryData.Value,6: ##.00;-##.00}%       {summaryData.Dividend,6: ##.00}%");
                }
                else
                {
                    if (!daysHeaderShown)
                    {
                        summarySB.AppendLine();
                        summarySB.AppendLine($"--- {chunks[1]} Days -------");
                        daysHeaderShown = true;
                    }

                    summarySB.AppendLine($"    {chunks[2]}   {summaryData.Value,6: ##.00;-##.00}% {summaryData.Time}");
                }
            }

            return summarySB.ToString();
        }
    }
}
