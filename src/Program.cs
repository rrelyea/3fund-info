using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace daily
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await QuoteFetcher.WritePricesToCsvPerYear(585, "vtsax", 2000);
            await QuoteFetcher.WritePricesToCsvPerYear(569, "vtiax", 2010);
            await QuoteFetcher.WritePricesToCsvPerYear(584, "vbltx", 2001);
            await QuoteFetcher.WritePricesToCsvPerYear(970, "vti", 2001);
            await QuoteFetcher.WritePricesToCsvPerYear(3369, "vxus", 2011);
            await QuoteFetcher.WritePricesToCsvPerYear(928, "bnd", 2007);

            await Output3FC("vtsax", "vtiax", "vbltx", 2011);
            await Output3FC("vti", "vxus", "bnd", 2012);
        }

        private static async Task Output3FC(string stockFund, string intlFund, string bondFund, int startYear)
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
                    StringBuilder summarySB = new StringBuilder();

                    for (int year = 2021; year >= startYear; year--)
                    {
                        double ytd = await quoteData[year - startYear].CalculatePerf(stock, intl, 100 - stock, year, summarySB);
                        summarySB.AppendLine($"{year} {ytd:0.##}%");
                        summarySB.AppendLine();
                    }

                    int bond = 100 - stock;
                    string outputFile = $"perf\\{stock}-{bond}\\{stock}-{bond} ({intl}% intl)-{stockFund.ToUpper()}-{bondFund.ToUpper()}-{intlFund.ToUpper()}.txt";
                    File.WriteAllText(outputFile, summarySB.ToString());
                }
            }
        }
    }
}
