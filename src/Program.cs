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

            QuoteData[] quoteData = new QuoteData[11];

            for (int year = 2011; year <= 2021; year++)
            {
                quoteData[year - 2011] = new QuoteData("vtsax", "vtiax", "vbltx", year);
            }

            for (int stock = 100; stock >= 0; stock -= 5)
            {
                for (int intl = 0; intl <= 50; intl += 10)
                {
                    StringBuilder summarySB = new StringBuilder();

                    for (int year = 2021; year >= 2011; year--)
                    {
                        double ytd = await quoteData[year - 2011].CalculatePerf(stock, intl, 100 - stock, year, summarySB);
                        summarySB.AppendLine($"{year} {ytd:0.##}%");
                        summarySB.AppendLine();
                    }

                    int bond = 100 - stock;
                    string outputFile = $"perf\\{stock}-{bond} ({intl}% intl)\\{stock}-{bond} ({intl}% intl)-VTSAX-VBLTX-VTIAX.txt";
                    File.WriteAllText(outputFile, summarySB.ToString());
                }
            }
        }
    }
}
