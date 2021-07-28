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

            // TODO: fix code so that we can start with a partial year, instead of only a full year.
            await threeFunds["Vanguard ETFs"].CreatePerfSummary(2012, marketTime);
            await threeFunds["Vanguard Mutual Funds"].CreatePerfSummary(2011, marketTime);
            await threeFunds["Fidelity Mutual Funds"].CreatePerfSummary(2019, marketTime);
        }

        private static Dictionary<string, ThreeFund> InitializeThreeFunds()
        {
            var threeFunds = new Dictionary<string, ThreeFund>();

            threeFunds.Add("Vanguard Mutual Funds", new ThreeFund("vtsax", "vtiax", "vbtlx", FundStyle.MutualFund, "Vanguard"));
            threeFunds.Add("Vanguard ETFs", new ThreeFund("vti", "vxus", "bnd", FundStyle.ETF, "Vanguard"));
            threeFunds.Add("Fidelity Mutual Funds", new ThreeFund("fzrox", "fzilx", "fxnax", FundStyle.MutualFund, "Fidelity"));

            return threeFunds;
        }
    }
}
