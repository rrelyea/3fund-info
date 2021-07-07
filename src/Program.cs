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

            for (int year = 2011; year <= 2021; year++)
            {
                var quoteData = new QuoteData("vtsax", "vtiax", "vbltx", year);
                for (int stock = 100; stock >= 0; stock -= 5)
                {
                    for (int intl = 0; intl <= 50; intl += 10)
                    {
                        await quoteData.CalculatePerf(stock, intl, 100 - stock, year);
                    }
                }
            }
        }
    }
}
