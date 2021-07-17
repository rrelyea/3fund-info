using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace daily
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(DateTime.Now.ToShortTimeString());
            Dictionary<string, ThreeFund> threeFunds = InitializeThreeFunds();
            MarketTime marketTime = MarketTimes.GetMarketTime();

            await threeFunds["Vanguard ETFs"].CreatePerfSummary(2012, marketTime);
            await threeFunds["Vanguard Mutual Funds"].CreatePerfSummary(2011, marketTime);
        }

        private static Dictionary<string, ThreeFund> InitializeThreeFunds()
        {
            var threeFunds = new Dictionary<string, ThreeFund>();

            threeFunds.Add("Vanguard Mutual Funds", new ThreeFund("vtsax", "vtiax", "vbtlx", FundStyle.MutualFund, "Vanguard"));
            threeFunds.Add("Vanguard ETFs", new ThreeFund("vti", "vxus", "bnd", FundStyle.ETF, "Vanguard"));

            return threeFunds;
        }
    }
}
