using System.Threading.Tasks;

namespace daily
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await QuoteScraper.WritePricesToCsvPerYear(585, "vtsax", 2000);
            await QuoteScraper.WritePricesToCsvPerYear(569, "vtiax", 2010);
            await QuoteScraper.WritePricesToCsvPerYear(584, "vbltx", 2001);
            await QuoteScraper.WritePricesToCsvPerYear(970, "vti", 2001);
        }
    }
}
