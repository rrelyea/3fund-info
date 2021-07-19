using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace daily.Formatters
{
    public class HtmlFormatter
    {
        static StringBuilder summarySB = new StringBuilder();

        public static async Task OutputHtmlFile(ThreeFund threeFund, int startYear, PerfCalculator perfCalc, int stock, int intl, int bond, Dictionary<string, FundValue> perfSummaries)
        {
            FileInfo outputFile = new FileInfo($"perf\\{stock}-{bond}\\{stock}-{bond} ({intl}% intl)-{threeFund.StockFund.UpperSymbol}-{threeFund.BondFund.UpperSymbol}-{threeFund.InternationStockFund.UpperSymbol}.html");
            if (!outputFile.Directory.Exists)
            {
                outputFile.Directory.Create();
            }

            summarySB.AppendLine("<html><body>");
            AppendDiv($"Performance for {stock}/{bond} ({intl}% intl)  {threeFund.StockFund.UpperSymbol}/{threeFund.BondFund.UpperSymbol} ({threeFund.InternationStockFund.UpperSymbol})");
            AppendDiv();
            AppendDiv("  Appreciation % |  Dividend %");
            CreateHtmlPerfBody(perfSummaries);
            summarySB.AppendLine("</body><html>");
            await File.WriteAllTextAsync(outputFile.FullName, summarySB.ToString());
            Console.Write(".");
        }

        private static void AppendDiv(string line = null)
        {
            summarySB.AppendLine($"<div>{line}</div>");
        }

        private static void CreateHtmlPerfBody(Dictionary<string, FundValue> perfSummaries)
        {
            string year = null;
            string currentYear = DateTime.Now.Year.ToString();
            bool daysHeaderShown = false;
            foreach (var date in perfSummaries.Keys)
            {
                string[] chunks = date.Split('-');
                if (chunks[0] != year)
                {
                    year = chunks[0];
                    AppendDiv();
                    AppendDiv("----------------------------------");
                    AppendDiv($"{year}:");
                }
                FundValue summaryData = perfSummaries[date];
                if (chunks.Length == 2)
                {
                    if (date == DateTime.Now.ToString("yyyy-MMM"))
                    {
                        AppendDiv("--------------------");
                    }
                    AppendDiv($"    {chunks[1]} {summaryData.Value,7: ##.00;-##.00}%         {summaryData.Dividend: ##.00}%");
                }
                else if (chunks.Length == 1)
                {
                    AppendDiv("==================================");
                    string ytdStr = year == currentYear ? "YTD " : "Year";
                    AppendDiv($"{chunks[0]} {ytdStr}{summaryData.Value,6: ##.00;-##.00}%       {summaryData.Dividend,6: ##.00}%");
                }
                else
                {
                    if (!daysHeaderShown)
                    {
                        AppendDiv();
                        AppendDiv($"--- {chunks[1]} Days -------");
                        daysHeaderShown = true;
                    }

                    AppendDiv($"    {chunks[2]}   {summaryData.Value,6: ##.00;-##.00}%");
                }
            }
        }
    }
}
