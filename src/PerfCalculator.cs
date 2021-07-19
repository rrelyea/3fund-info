using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace daily
{
    public class PerfCalculator

    {
        public ThreeFund ThreeFund { get; private set; }

        public PerfCalculator(ThreeFund threeFund)
        {
            ThreeFund = threeFund;
        }

        internal void CalculateMonthlyAndYearlyPerf(double stock, double intl, double bond, int year, Dictionary<string, FundValue> perfSummaries)
        {
            double bondPct = bond / 100.0;
            double intlPct = stock / 100.0 * intl / 100.0;
            double stockPct = stock / 100.0 * (100 - intl) / 100.0;

            double finalYtd = double.NaN;
            FundValue[] stockClose = new FundValue[13];
            FundValue[] intlClose = new FundValue[13];
            FundValue[] bondClose = new FundValue[13];

            double lastStockPrice = double.NaN;
            double lastIntlPrice = double.NaN;
            double lastBondPrice = double.NaN;
            double lastMTD = double.NaN;
            int lastMonth = 0;
            int month = 0;
            StringBuilder daysSection = new StringBuilder();
            DateTime lastDate = DateTime.MinValue;
            bool storedOpenPrice = false;
            double stockDividendMTD = 0.0;
            double intlDividendMTD = 0.0;
            double bondDividendMTD = 0.0;
            double stockDividendYTD = 0.0;
            double intlDividendYTD = 0.0;
            double bondDividendYTD = 0.0;
            foreach (var date in ThreeFund.StockFund.FundValues[year].Keys)
            {
                month = date.Month;
                if (!storedOpenPrice)
                {
                    stockClose[0] = ThreeFund.StockFund.FundValues[year][date];
                    intlClose[0] = ThreeFund.InternationStockFund.FundValues[year][date];
                    bondClose[0] = ThreeFund.BondFund.FundValues[year][date];
                    lastStockPrice = stockClose[0].Value;
                    lastIntlPrice = intlClose[0].Value;
                    lastBondPrice = bondClose[0].Value;
                    storedOpenPrice = true;
                }

                if (year == date.Year)
                {
                    if (lastMonth < month && month > 1)
                    {
                        double mtdDividendPct1 = stockPct * stockDividendMTD + intlPct * intlDividendMTD + bondPct * bondDividendMTD;

                        string dateKey1 = lastDate.ToString("yyyy-MMM", CultureInfo.InvariantCulture);
                        perfSummaries.Add(dateKey1, new FundValue() { Value = lastMTD, Dividend = mtdDividendPct1 });

                        stockDividendYTD += stockDividendMTD;
                        intlDividendYTD += intlDividendMTD;
                        bondDividendYTD += bondDividendMTD;
                        stockDividendMTD = 0.0;
                        intlDividendMTD = 0.0;
                        bondDividendMTD = 0.0;
                    }

                    var stockPerf = calculateDaysPerf(stockClose, ThreeFund.StockFund.FundValues[year], month, lastStockPrice, date);
                    var intlPerf = calculateDaysPerf(intlClose, ThreeFund.InternationStockFund.FundValues[year], month, lastIntlPrice, date);
                    var bondPerf = calculateDaysPerf(bondClose, ThreeFund.BondFund.FundValues[year], month, lastBondPrice, date);

                    lastStockPrice = stockPerf.Item4;
                    lastIntlPrice = intlPerf.Item4;
                    lastBondPrice = bondPerf.Item4;

                    double ytd = stockPct * stockPerf.Item1 + intlPct * intlPerf.Item1 + bondPct * bondPerf.Item1;
                    double mtd = stockPct * stockPerf.Item2 + intlPct * intlPerf.Item2 + bondPct * bondPerf.Item2;
                    stockDividendMTD += stockPerf.Item6;
                    intlDividendMTD += intlPerf.Item6;
                    bondDividendMTD += bondPerf.Item6;
                    double day = stockPct * stockPerf.Item3 + intlPct * intlPerf.Item3 + bondPct * bondPerf.Item3;

                    if (year == DateTime.Now.Year && month == DateTime.Now.Month)
                    {
                        bool interim = stockPerf.Item5 != null || intlPerf.Item5 != null || bondPerf.Item5 != null;
                        string interimStr = null;
                        if (interim)
                        {
                            TimeSpan captureTime = date.AddHours(3).TimeOfDay;
                            interimStr = $" *{captureTime.Hours}:{captureTime.Minutes:00} ET";
                        }

                        perfSummaries.Add(date.ToString("yyyy-MMM-dd"), new FundValue() { Value = day, Time = interimStr });
                    }

                    lastMTD = mtd;
                    lastMonth = month;
                    finalYtd = ytd;
                }

                lastDate = date;
            }

            double mtdDividendPct2 = stockPct * stockDividendMTD + intlPct * intlDividendMTD + bondPct * bondDividendMTD;

            string dateKey2 = lastDate.ToString("yyyy-MMM", CultureInfo.InvariantCulture);
            perfSummaries.Add(dateKey2, new FundValue() { Value = lastMTD, Dividend = mtdDividendPct2 });

            stockDividendYTD += stockDividendMTD;
            intlDividendYTD += intlDividendMTD;
            bondDividendYTD += bondDividendMTD;

            double finalYtdDiv = stockPct * stockDividendYTD + intlPct * intlDividendYTD + bondPct * bondDividendYTD;

            string dateKey3 = lastDate.ToString("yyyy", CultureInfo.InvariantCulture);
            perfSummaries.Add(dateKey3, new FundValue() { Value = finalYtd, Dividend = finalYtdDiv });
        }

        private static Tuple<double, double, double, double, string, double> calculateDaysPerf(FundValue[] monthlyCloses, Dictionary<DateTime, FundValue> dailyPrices, int index, double lastPrice, DateTime date)
        {
            if (monthlyCloses[index] == null)
            {
                monthlyCloses[index] = new FundValue();
            }

            monthlyCloses[index].Value = dailyPrices[date].Value;

            double ytd = (monthlyCloses[index].Value - monthlyCloses[0].Value) / monthlyCloses[0].Value * 100.0;
            double mtd = (monthlyCloses[index].Value - monthlyCloses[index - 1].Value) / monthlyCloses[index - 1].Value * 100.0;
            double day = (monthlyCloses[index].Value - lastPrice) / lastPrice * 100.0;
            return new Tuple<double, double, double, double, string, double>(ytd, mtd, day, monthlyCloses[index].Value, dailyPrices[date].Time, dailyPrices[date].Dividend);
        }
    }
}
