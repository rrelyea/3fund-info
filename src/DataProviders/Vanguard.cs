using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace daily.DataProviders
{
    public static class Vanguard
    {
        public static void LoadPricesIntoFund(int fundId, Fund fund, int beginYear)
        {
            Console.Write(fund.Symbol);
            for (int year = beginYear; year <= DateTime.Now.Year; year++)
            {
                Console.Write($" {year}");
                string beginDate = Uri.EscapeUriString(new DateTime(year - 1, 12, 28).ToShortDateString());
                string endDate = Uri.EscapeUriString(new DateTime(year, 12, 31).ToShortDateString());

                string fundType = fundId < 900 ? "VanguardFunds" : "ExchangeTradedShares";
                string url = $"https://personal.vanguard.com/us/funds/tools/pricehistorysearch?radio=1&results=get&FundType={fundType}&FundId={fundId}&Sc=1&radiobutton2=1&beginDate={beginDate}&endDate={endDate}&year=#res";

                var response = CallUrl(url).Result;
                ParseHtml(fund.FundValues, response, year);
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

        private static void ParseHtml(Dictionary<int, FundValues> yearlyValues, string html, int year)
        {
            FundValues fundValues = new FundValues();
            yearlyValues[year] = fundValues;
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
                                wroteHeader = true;
                            }
                        }
                        else
                        {
                            DateTime date;
                            bool okDate = DateTime.TryParse(c0, out date);
                            if (okDate && date.Year == year)
                            {
                                DateTime previousDate = DateTime.Parse(previousNode0);
                                if (previousNode0 != null && previousNode0 != "Date" && (previousDate.Year == year - 1) && !wroteLastYearClose)
                                {
                                    fundValues.Add(previousDate, new FundValue() { Value = double.Parse(previousNode1.Substring(1)) });
                                    wroteLastYearClose = true;
                                }

                                fundValues.Add(date, new FundValue() { Value = double.Parse(c1.Substring(1))});
                            }
                        }
                    }
                }
            }
        }
    }
}
