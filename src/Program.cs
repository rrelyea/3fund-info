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

            QuoteData[] quoteData = new QuoteData[11];

            for (int year = 2011; year <= 2021; year++)
            {
                quoteData[year - 2011] = new QuoteData("vtsax", "vtiax", "vbltx", year);
            }

            for (int stock = 100; stock >= 0; stock -= 5)
            {
                for (int intl = 0; intl <= 50; intl += 10)
                {
                    for (int year = 2021; year >= 2011; year++)
                    {
                        StringBuilder sb = new StringBuilder();
                        int bond = 100 - stock;
                        string outputFile = $"perf\\vanguard\\us {stock}-bond {bond}-intl {intl}\\us {stock}-bond {bond}-intl {intl}-Summary.txt";

                        double ytd = await quoteData[year-2011].CalculatePerf(stock, intl, 100 - stock, year);
                        sb.AppendLine($"{year} {ytd}");

                        File.WriteAllText(outputFile, sb.ToString());
                    }
                }
            }
        }
    }
}
