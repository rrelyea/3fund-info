using daily.DataProviders;
using daily.Formatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace daily
{
    public class ThreeFund
    {
        public ThreeFund(string stockSymbol, string internationalSymbol, string bondSymbol, FundStyle fundStyle, string fundSource)
        {
            StockFund = new Fund(stockSymbol);
            InternationStockFund = new Fund(internationalSymbol);
            BondFund = new Fund(bondSymbol);

            FundStyle = fundStyle;
            FundSource = fundSource;
        }
        public Fund StockFund { get; private set; }
        public Fund InternationStockFund { get; private set; }
        public Fund BondFund { get; private set; }

        public FundStyle FundStyle { get; private set; }
        public string FundSource { get; private set; }

        public async Task CreatePerfSummary(int startYear, MarketTime marketTime)
        {
            if (marketTime != MarketTime.None)
            {
                Console.WriteLine();
                switch (FundSource)
                {
                    case "Vanguard":
                        await Vanguard.LoadPricesIntoFund(StockFund, startYear, refetchCurrentYear: (marketTime == MarketTime.VanguardHistoricalPricesUpdated));
                        await Vanguard.LoadPricesIntoFund(InternationStockFund, startYear, refetchCurrentYear: (marketTime == MarketTime.VanguardHistoricalPricesUpdated));
                        await Vanguard.LoadPricesIntoFund(BondFund, startYear, refetchCurrentYear: (marketTime == MarketTime.VanguardHistoricalPricesUpdated));
                        break;
                    default:
                        var avS = new AlphaVantage(StockFund.Symbol, TimeSeries.Monthly);
                        LoadPricesIntoFund(avS, StockFund);

                        var avIS = new AlphaVantage(InternationStockFund.Symbol, TimeSeries.Monthly);
                        LoadPricesIntoFund(avIS, InternationStockFund);

                        var avB = new AlphaVantage(BondFund.Symbol, TimeSeries.Monthly);
                        LoadPricesIntoFund(avB, BondFund);
                        break;
                }
            }

            if ((marketTime == MarketTime.Open && FundStyle == FundStyle.ETF) || (marketTime == MarketTime.MutualFundPricesPublished && FundStyle == FundStyle.MutualFund))
            {
                TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone);
                MartketWatch.LoadRealTimePriceIntoFund(StockFund, now);
                MartketWatch.LoadRealTimePriceIntoFund(InternationStockFund, now);
                MartketWatch.LoadRealTimePriceIntoFund(BondFund, now);
            }

            if ((marketTime == MarketTime.Open && FundStyle == FundStyle.ETF)
                || (marketTime == MarketTime.MutualFundPricesPublished && FundStyle == FundStyle.MutualFund)
                || (marketTime == MarketTime.VanguardHistoricalPricesUpdated)
                || (marketTime == MarketTime.MarketClosed)
                || (marketTime == MarketTime.MarketClosedAllDay))

            {
                Console.Write("Calculating perf:");
                await CalculateAndOutputPerfSummaries(startYear);
            }
        }

        private void LoadPricesIntoFund(AlphaVantage avS, Fund fund)
        {
            foreach (var dayData in avS.GetDataRoot().Result.EnumerateObject())
            {
                string dateKey = dayData.Name;
                string close = dayData.Value.GetProperty("4. close").GetString();
                string dividend = dayData.Value.GetProperty("7. dividend amount").GetString();
                DateTime dateTimeKey = DateTime.Parse(dateKey);

                if (dateTimeKey.Month == 12)
                {
                    StoreValues(fund, close, dividend, dateTimeKey, dateTimeKey.Year + 1);
                }

                StoreValues(fund, close, dividend, dateTimeKey, dateTimeKey.Year);
            }
        }

        private static void StoreValues(Fund fund, string close, string dividend, DateTime dateTimeKey, int year)
        {
            YearValues yearValues = null;
            bool found = fund.FundValues.TryGetValue(year, out yearValues);
            if (!found)
            {
                yearValues = new YearValues();
                fund.FundValues[year] = yearValues;
            }

            yearValues.Add(dateTimeKey, new FundValue() { Value = double.Parse(close), Dividend = double.Parse(dividend) });
        }

        public async Task CalculateAndOutputPerfSummaries(int startYear)
        {
            var perfCalc = new PerfCalculator(this);

            for (int stock = 100; stock >= 0; stock -= 5)
            {
                for (int intl = 0; intl <= 50; intl += 10)
                {
                    int bond = 100 - stock;
                    var perfSummaries = new Dictionary<string, FundValue>();

                    for (int year = 2021; year >= startYear; year--)
                    {
                        perfCalc.CalculateMonthlyAndYearlyPerf(stock, intl, 100 - stock, year, perfSummaries);
                    }

                    await TextFormatter.OutputTextFile(this, startYear, perfCalc, stock, intl, bond, perfSummaries);
                    await HtmlFormatter.OutputHtmlFile(this, startYear, perfCalc, stock, intl, bond, perfSummaries);
                }
            }

            Console.WriteLine();
        }
    }
}
