using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace daily
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Collecting prices:");
            await QuoteFetcher.WritePricesToCsvPerYear(585, "vtsax", 2000);
            await QuoteFetcher.WritePricesToCsvPerYear(569, "vtiax", 2010);
            await QuoteFetcher.WritePricesToCsvPerYear(584, "vbltx", 2001);
            await QuoteFetcher.WritePricesToCsvPerYear(970, "vti", 2001);
            await QuoteFetcher.WritePricesToCsvPerYear(3369, "vxus", 2011);
            await QuoteFetcher.WritePricesToCsvPerYear(928, "bnd", 2007);

            Console.Write("Calculating perf");
            await OutputThreeFundPerfSummary("vtsax", "vtiax", "vbltx", startYear:2011);
            await OutputThreeFundPerfSummary("vti", "vxus", "bnd", startYear:2012);
        }

        private static async Task OutputThreeFundPerfSummary(string stockFund, string intlFund, string bondFund, int startYear)
        {
            QuoteData[] quoteData = new QuoteData[2021-startYear+1];

            for (int year = startYear; year <= 2021; year++)
            {
                quoteData[year - startYear] = new QuoteData(stockFund, intlFund, bondFund, year);
            }

            for (int stock = 100; stock >= 0; stock -= 5)
            {
                for (int intl = 0; intl <= 50; intl += 10)
                {
                    int bond = 100 - stock;

                    StringBuilder summarySB = new StringBuilder();
                    summarySB.AppendLine($"Performance for {stock}-{bond} ({intl}% intl)-{stockFund.ToUpper()}-{bondFund.ToUpper()}-{intlFund.ToUpper()}");
                    summarySB.AppendLine();
                    summarySB.AppendLine("Yearly      | Monthly     | Daily");
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

                    string outputFile = $"perf\\{stock}-{bond}\\{stock}-{bond} ({intl}% intl)-{stockFund.ToUpper()}-{bondFund.ToUpper()}-{intlFund.ToUpper()}.txt";
                    await File.WriteAllTextAsync(outputFile, summarySB.ToString());
                    Console.Write(".");
                }
            }

            Console.WriteLine();
        }
    }
}
