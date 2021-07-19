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
            summarySB.Clear();
            FileInfo outputFile = new FileInfo($"perf\\{stock}-{bond}\\{stock}-{bond} ({intl}% intl)-{threeFund.StockFund.UpperSymbol}-{threeFund.BondFund.UpperSymbol}-{threeFund.InternationStockFund.UpperSymbol}.html");
            if (!outputFile.Directory.Exists)
            {
                outputFile.Directory.Create();
            }

            summarySB.AppendLine("<html>");
            summarySB.AppendLine("<head>");
            summarySB.AppendLine("<style> .right { text-align: right; } </style>");
            summarySB.AppendLine("</head>");
            summarySB.AppendLine("<body>");
            AppendDiv($"Performance for {stock}/{bond} ({intl}% intl)  {threeFund.StockFund.UpperSymbol}/{threeFund.BondFund.UpperSymbol} ({threeFund.InternationStockFund.UpperSymbol})");
            AppendDiv();
            CreateHtmlPerfBody(perfSummaries);
            summarySB.AppendLine("</body><html>");
            await File.WriteAllTextAsync(outputFile.FullName, summarySB.ToString());
            Console.Write(".");
        }

        private static void AppendDiv(string line = "&nbsp")
        {
            summarySB.AppendLine($"<div>{line}</div>");
        }
        private static void AppendRow()
        {
            summarySB.AppendLine($"<tr><td>&nbsp;</td></tr>");
        }
        private static void StartTable()
        {
            summarySB.AppendLine($"<table>");
        }
        private static void EndTable()
        {
            summarySB.AppendLine($"</table>");
        }
        private static void Append3Cells(string c0, string c1 = null, string c2 = null)
        {
            summarySB.AppendLine($"<tr><td class=right>{c0}</td><td class=right>{c1}</td><td class=right>{c2}</td></tr>");
        }

        private static void CreateHtmlPerfBody(Dictionary<string, FundValue> perfSummaries)
        {
            string year = null;
            string currentYear = DateTime.Now.Year.ToString();
            bool daysHeaderShown = false;
            StartTable();
            Append3Cells("", "Appreciation %", "Dividend %");

            foreach (var date in perfSummaries.Keys)
            {
                string[] chunks = date.Split('-');
                if (chunks[0] != year)
                {
                    year = chunks[0];
                    AppendRow();
                    Append3Cells($"{year}:");
                }
                FundValue summaryData = perfSummaries[date];
                if (chunks.Length == 2)
                {
                    if (date == DateTime.Now.ToString("yyyy-MMM"))
                    {
                        AppendRow();
                    }
                    
                    Append3Cells(chunks[1], $"{summaryData.Value,7: ##.00;-##.00}%", $"{summaryData.Dividend:##.00}%");
                }
                else if (chunks.Length == 1)
                {
                    string ytdStr = year == currentYear ? "YTD " : "Year";
                    Append3Cells($"{chunks[0]} {ytdStr}", $"{summaryData.Value,6: ##.00;-##.00}%", $"{summaryData.Dividend,6: ##.00}%");
                }
                else
                {
                    if (!daysHeaderShown)
                    {
                        AppendRow();
                        Append3Cells($"{chunks[1]} Days");
                        daysHeaderShown = true;
                    }

                    Append3Cells($"{chunks[2]}", $"{summaryData.Value,6: ##.00;-##.00}%", $"{summaryData.Time}");
                }
             
            }
            
            AppendRow();
            EndTable();
        }
    }
}
