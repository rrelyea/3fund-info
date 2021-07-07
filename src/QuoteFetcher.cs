﻿using HtmlAgilityPack;
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
        public static async Task WritePricesToCsvPerYear(int fundId, string fundName, int beginYear)
        {
            bool recreateAll = true;
            string lastQuote = null;

            Console.Write(fundName);
            for (int year = beginYear; year <= DateTime.Now.Year; year++)
            {
                FileInfo filePath = new FileInfo(Path.Combine(System.Environment.CurrentDirectory, "prices", "vanguard", fundName, $"{fundName}-{year}.csv"));

                if (recreateAll || !filePath.Exists || year == DateTime.Now.Year)
                {
                    string beginDate = Uri.EscapeUriString(lastQuote == null ? new DateTime(year, 1, 1).ToShortDateString() : lastQuote);
                    string endDate = Uri.EscapeUriString(new DateTime(year, 12, 31).ToShortDateString());

                    string fundType = fundId < 900 ? "VanguardFunds" : "ExchangeTradedShares";
                    string url = $"https://personal.vanguard.com/us/funds/tools/pricehistorysearch?radio=1&results=get&FundType={fundType}&FundId={fundId}&Sc=1&radiobutton2=1&beginDate={beginDate}&endDate={endDate}&year=#res";

                    var response = CallUrl(url).Result;
                    Tuple<StringBuilder, string> result = ParseHtml(response);
                    lastQuote = result.Item2;

                    if (result.Item1 != null)
                    {
                        if (!filePath.Directory.Exists)
                        {
                            filePath.Directory.Create();
                        }

                        await File.WriteAllTextAsync(filePath.FullName, result.Item1.ToString());

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

        private static Tuple<StringBuilder, string> ParseHtml(string html)
        {
            string lastDate = null;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var tables = htmlDoc.DocumentNode.Descendants("table");
            List<HtmlNode> dataTables = tables.Where(node => node.GetAttributeValue("class", "").Contains("dataTable")).ToList();

            StringBuilder sb = null;

            if (dataTables.Count > 1)
            {
                bool collectRows = false;
                foreach (HtmlNode row in dataTables[1].SelectNodes("//tbody//tr"))
                {
                    if (row.ChildNodes.Count >= 2)
                    {
                        if (sb == null)
                        {
                            sb = new StringBuilder();
                        }

                        var c0 = row.ChildNodes[0].InnerText;
                        var c1 = row.ChildNodes[1].InnerText;

                        if (c0 == "Date")
                        {
                            collectRows = true;
                        }

                        if (collectRows)
                        {
                            sb.AppendLine(c0 + "," + c1);
                            lastDate = c0;
                        }
                    }
                }
            }

            return new Tuple<StringBuilder, string>(sb, lastDate);
        }
    }
}