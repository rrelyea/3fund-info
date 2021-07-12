using System;
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

        public async Task WritePricesToCsvPerYear(int beginYear)
        {
            await QuoteFetcher.WritePricesToCsvPerYear(LookupFundId(StockFund), StockFund, beginYear);
            await QuoteFetcher.WritePricesToCsvPerYear(LookupFundId(InternationStockFund), InternationStockFund, beginYear);
            await QuoteFetcher.WritePricesToCsvPerYear(LookupFundId(BondFund), BondFund, beginYear);
        }

        public async Task OutputThreeFundPerfSummary(int startYear)
        {
            QuoteData[] quoteData = new QuoteData[2021 - startYear + 1];

            for (int year = startYear; year <= 2021; year++)
            {
                quoteData[year - startYear] = new QuoteData(this, year);
            }

            for (int stock = 100; stock >= 0; stock -= 5)
            {
                for (int intl = 0; intl <= 50; intl += 10)
                {
                    int bond = 100 - stock;

                    StringBuilder summarySB = new StringBuilder();
                    summarySB.AppendLine($"Performance for {stock}/{bond} ({intl}% intl)-{this.StockFund.UpperSymbol}-{this.BondFund.UpperSymbol}-{this.InternationStockFund.UpperSymbol}");
                    summarySB.AppendLine();
                    summarySB.AppendLine("      Year % |     Month % |          Day %");
                    summarySB.AppendLine();
                    for (int year = 2021; year >= startYear; year--)
                    {
                        summarySB.AppendLine("-------------------------------------------");
                        summarySB.AppendLine($"{year}:");
                        double ytd = quoteData[year - startYear].CalculatePerf(stock, intl, 100 - stock, year, summarySB);
                        summarySB.AppendLine("===========================================");
                        summarySB.AppendLine($"{year}{ytd,7:0.00}%");
                        summarySB.AppendLine();
                    }

                    FileInfo outputFile = new FileInfo($"perf\\{stock}-{bond}\\{stock}-{bond} ({intl}% intl)-{this.StockFund.UpperSymbol}-{this.BondFund.UpperSymbol}-{this.InternationStockFund.UpperSymbol}.txt");
                    if (!outputFile.Directory.Exists)
                    {
                        outputFile.Directory.Create();
                    }

                    await File.WriteAllTextAsync(outputFile.FullName, summarySB.ToString());
                    Console.Write(".");
                }
            }

            Console.WriteLine();
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
