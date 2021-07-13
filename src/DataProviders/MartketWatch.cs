using HtmlAgilityPack;
using System;
using System.Linq;

namespace daily.DataProviders
{
    public class MartketWatch
    {
        public static void LoadRealTimePriceIntoFund(Fund fund, DateTime now)
        {
            string url = $"https://www.marketwatch.com/investing/fund/{fund.UpperSymbol}";

            var response = HttpUtility.CallUrl(url).Result;
            ParseHtml(fund.FundValues[now.Year], response, now);
        }

        private static void ParseHtml(YearValues yearValues, string html, DateTime now)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var quotes = htmlDoc.DocumentNode.Descendants("h3").Where(node => node.GetAttributeValue("class","").Contains("intraday__price")).ToList();
            string quote = quotes[0].InnerText.Trim();

            DateTime lastDate = yearValues.Keys.Last<DateTime>();
            if (now.Date != lastDate.Date)
            {
                yearValues.Add(now, new FundValue() { Value = double.Parse(quote.Substring(1)), Interim = true });
            }
        }
    }
}