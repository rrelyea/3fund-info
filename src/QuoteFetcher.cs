using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace daily
{
    public static class QuoteFetcher
    {
        public static async Task WritePricesToCsvPerYear(int fundId, Fund fund, int beginYear)
        {
            bool recreateAll = false;

            Console.Write(fund.Symbol);
            for (int year = beginYear; year <= DateTime.Now.Year; year++)
            {
                FileInfo filePath = new FileInfo(Path.Combine(System.Environment.CurrentDirectory, "prices", fund.Symbol, $"{fund.Symbol}-{year}.csv"));

                if (recreateAll || !filePath.Exists || year == DateTime.Now.Year)
                {
                    string beginDate = Uri.EscapeUriString(new DateTime(year - 1, 12, 28).ToShortDateString());
                    string endDate = Uri.EscapeUriString(new DateTime(year, 12, 31).ToShortDateString());

                    string fundType = fundId < 900 ? "VanguardFunds" : "ExchangeTradedShares";
                    string url = $"https://personal.vanguard.com/us/funds/tools/pricehistorysearch?radio=1&results=get&FundType={fundType}&FundId={fundId}&Sc=1&radiobutton2=1&beginDate={beginDate}&endDate={endDate}&year=#res";

                    var response = CallUrl(url).Result;
                    StringBuilder sb = ParseHtml(response, year);

                    if (sb != null)
                    {
                        if (!filePath.Directory.Exists)
                        {
                            filePath.Directory.Create();
                        }

                        await File.WriteAllTextAsync(filePath.FullName, sb.ToString());

                        Console.Write($" {year}");
                    }
                }
            }

            Console.WriteLine();
        }

        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            client.DefaultRequestHeaders.Accept.Clear();
            string response = await client.GetStringAsync(fullUrl);
            return response;
        }

        private static StringBuilder ParseHtml(string html, int year)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var tables = htmlDoc.DocumentNode.Descendants("table");
            List<HtmlNode> dataTables = tables.Where(node => node.GetAttributeValue("class", "").Contains("dataTable")).ToList();

            StringBuilder sb = null;

            if (dataTables.Count > 1)
            {
                bool wroteHeader = false;
                bool wroteLastYearClose = false;
                string previousNode0 = null;
                string previousNode1 = null;
                string c0 = null;
                string c1 = null;
                foreach (HtmlNode row in dataTables[1].SelectNodes("//tbody//tr"))
                {
                    if (row.ChildNodes.Count >= 2)
                    {
                        if (sb == null)
                        {
                            sb = new StringBuilder();
                        }

                        previousNode0 = c0;
                        previousNode1 = c1;

                        c0 = row.ChildNodes[0].InnerText;
                        c1 = row.ChildNodes[1].InnerText;

                        if (!wroteHeader)
                        {
                            if (c0 == "Date")
                            {
                                sb.AppendLine($"{c0},{c1}");
                                wroteHeader = true;
                            }
                        }
                        else
                        {
                            DateTime date;
                            bool okDate = DateTime.TryParse(c0, out date);
                            if (okDate && date.Year == year)
                            {
                                if (previousNode0 != null && previousNode0 != "Date" && (DateTime.Parse(previousNode0).Year == year - 1) && !wroteLastYearClose)
                                {
                                    sb.AppendLine($"{previousNode0},{previousNode1}");
                                    wroteLastYearClose = true;
                                }

                                sb.AppendLine($"{c0},{c1}");
                            }
                        }
                    }
                }
            }

            return sb;
        }
    }
}