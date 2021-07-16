using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace daily.DataProviders
{
    public static class Vanguard
    {
        public async static Task LoadPricesIntoFund(Fund fund, int beginYear, bool refetchCurrentYear)
        {
            int fundId = LookupFundId(fund);
            string fundType = fundId < 900 ? "VanguardFunds" : "ExchangeTradedShares";
            bool dividendDataFetched = false;
            JsonElement dataRoot = new JsonElement();
            Console.Write($"Fetching [{fund.Symbol}]");

            for (int year = beginYear; year <= DateTime.Now.Year; year++)
            {
                FileInfo yearDataFile = new FileInfo($"prices\\{fund.Symbol}\\{fund.Symbol}-{year}.csv");
                fund.FundValues[year] = new YearValues();

                bool isCurrentYear = DateTime.Now.Year == year;
                if (!yearDataFile.Exists || (refetchCurrentYear && isCurrentYear))
                {
                    if (dividendDataFetched)
                    {
                        // get dividend data
                        JsonElement alphaVantageMontlyData = await AlphaVantage.FetchAllData(fund.Symbol, TimeSeries.Monthly);
                        try
                        {
                            dataRoot = AlphaVantage.GetDataRoot(alphaVantageMontlyData, TimeSeries.Monthly);
                            dividendDataFetched = true;
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("dividend data - API allowance exceeded.");
                        }
                    }

                    Console.Write($" {year}");
                    string beginDate = Uri.EscapeUriString(new DateTime(year - 1, 12, 28).ToShortDateString());
                    string endDate = Uri.EscapeUriString(new DateTime(year, 12, 31).ToShortDateString());
                    string url = $"https://personal.vanguard.com/us/funds/tools/pricehistorysearch?radio=1&results=get&FundType={fundType}&FundId={fundId}&Sc=1&radiobutton2=1&beginDate={beginDate}&endDate={endDate}&year=#res";
                    var htmlResponse = HttpUtility.CallUrl(url).Result;
                    ParseHtml(fund.FundValues[year], htmlResponse, year);

                    var csvOutputContents = new StringBuilder();
                    foreach (var date in fund.FundValues[year].Keys)
                    {
                        JsonElement dateData;

                        string dividendStr = null;
                        if (dataRoot.ValueKind != JsonValueKind.Undefined)
                        {
                            bool foundData = dataRoot.TryGetProperty(date.ToString("yyyy-MM-dd"), out dateData);
                            if (foundData)
                            {
                                dividendStr = "," + dateData.GetProperty("7. dividend amount").GetString();
                            }
                        }

                        csvOutputContents.AppendLine($"{date.ToShortDateString()},{fund.FundValues[year][date].Value}{dividendStr}");
                    }

                    if (!yearDataFile.Directory.Exists)
                    {
                        yearDataFile.Directory.Create();
                    }

                    File.WriteAllText(yearDataFile.FullName, csvOutputContents.ToString());
                }
                else
                {
                    foreach (var line in File.ReadAllLines(yearDataFile.FullName))
                    {
                        string[] chunks = line.Split(',');
                        DateTime date = DateTime.Parse(chunks[0]);
                        FundValue fundValue = new FundValue() { Value = double.Parse(chunks[1]) };
                        if (chunks.Length == 3)
                        {
                            fundValue.Dividend = double.Parse(chunks[2]);
                        }

                        fund.FundValues[year].Add(date, fundValue);
                    }
                }
            }

            Console.WriteLine($" Last: {fund.FundValues[DateTime.Now.Year].Keys.Last<DateTime>().ToShortDateString()}");
        }

        private static void ParseHtml(YearValues yearValues, string html, int year)
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
                                    yearValues.Add(previousDate, new FundValue() { Value = double.Parse(previousNode1.Substring(1)) });
                                    wroteLastYearClose = true;
                                }

                                yearValues.Add(date, new FundValue() { Value = double.Parse(c1.Substring(1)) });
                            }
                        }
                    }
                }
            }
        }

        private static int LookupFundId(Fund fund)
        {
            switch (fund.Symbol)
            {
                case "vti": return 970;
                case "vxus": return 3369;
                case "bnd": return 928;
                case "vtsax": return 585;
                case "vtiax": return 569;
                case "vbtlx": return 584;
                default: return -1;
            }
        }
    }
}
