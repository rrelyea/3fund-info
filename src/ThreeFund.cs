using daily.DataProviders;
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
                await Vanguard.LoadPricesIntoFund(StockFund, startYear, refetchCurrentYear: (marketTime == MarketTime.VanguardHistoricalPricesUpdated));
                await Vanguard.LoadPricesIntoFund(InternationStockFund, startYear, refetchCurrentYear: (marketTime == MarketTime.VanguardHistoricalPricesUpdated));
                await Vanguard.LoadPricesIntoFund(BondFund, startYear, refetchCurrentYear: (marketTime == MarketTime.VanguardHistoricalPricesUpdated));
            }

            if ((marketTime == MarketTime.Open && FundStyle == FundStyle.ETF) || (marketTime == MarketTime.MutualFundPricesPublished && FundStyle == FundStyle.MutualFund))
            {
                DateTime now = DateTime.Now;
                MartketWatch.LoadRealTimePriceIntoFund(StockFund, now);
                MartketWatch.LoadRealTimePriceIntoFund(InternationStockFund, now);
                MartketWatch.LoadRealTimePriceIntoFund(BondFund, now);
            }

            if ((marketTime == MarketTime.Open && FundStyle == FundStyle.ETF)
                || (marketTime == MarketTime.MutualFundPricesPublished && FundStyle == FundStyle.MutualFund)
                || (marketTime == MarketTime.VanguardHistoricalPricesUpdated)
                || (marketTime == MarketTime.MarketClosedAllDay))

            {
                Console.Write("Calculating perf:");
                await OutputThreeFundPerfSummary(startYear);
            }
        }

        public async Task OutputThreeFundPerfSummary(int startYear)
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

                    await OutputTextFile(startYear, perfCalc, stock, intl, bond, perfSummaries);
                }
            }

            Console.WriteLine();
        }

        private async Task OutputTextFile(int startYear, PerfCalculator perfCalc, int stock, int intl, int bond, Dictionary<string, FundValue> perfSummaries)
        {
            FileInfo outputFile = new FileInfo($"perf\\{stock}-{bond}\\{stock}-{bond} ({intl}% intl)-{this.StockFund.UpperSymbol}-{this.BondFund.UpperSymbol}-{this.InternationStockFund.UpperSymbol}.txt");
            if (!outputFile.Directory.Exists)
            {
                outputFile.Directory.Create();
            }

            StringBuilder summarySB = new StringBuilder();
            summarySB.AppendLine($"Performance for {stock}/{bond} ({intl}% intl)  {this.StockFund.UpperSymbol}/{this.BondFund.UpperSymbol} ({this.InternationStockFund.UpperSymbol})");
            summarySB.AppendLine();
            summarySB.AppendLine("  Appreciation % |  Dividend %");
            summarySB.AppendLine(perfCalc.CreateTextPerfSummary(perfSummaries));
            await File.WriteAllTextAsync(outputFile.FullName, summarySB.ToString());
            Console.Write(".");
        }
    }
}
