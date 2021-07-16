using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace daily
{
    public class PerfCalculator

    {
        public ThreeFund ThreeFund { get; private set; }
        public int Year { get; private set; }

        public PerfCalculator(ThreeFund threeFund, int year)
        {
            ThreeFund = threeFund;
            Year = year;
        }

        internal Tuple<double,double> OutputPerfForOneYear(double stock, double intl, double bond, int year, StringBuilder summarySB)
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
            summarySB.AppendLine("-------------------------------------------");
            summarySB.AppendLine($"{year}:");
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
                        double mtdDividendPct1 = stockPct * stockDividendMTD + intlPct *intlDividendMTD + bondPct * bondDividendMTD;
                        summarySB.AppendLine($"    {lastDate.ToString("MMM", CultureInfo.InvariantCulture)} {lastMTD,7: ##.00;-##.00}%         {mtdDividendPct1: ##.00}%");
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
                        bool interim = stockPerf.Item5 || intlPerf.Item5 || bondPerf.Item5;
                        string interimStr = "";
                        if (interim)
                        {
                            TimeSpan captureTime = date.AddHours(3).TimeOfDay;
                            interimStr = $" *{captureTime.Hours}:{captureTime.Minutes:00} ET";
                        }

                        daysSection.AppendLine($"                                 {date.ToString("MMM", CultureInfo.InvariantCulture)} {date.Day:00} {day,7: ##.00;-##.00}%{interimStr}");
                    }

                    lastMTD = mtd;
                    lastMonth = month;
                    finalYtd = ytd;
                }

                lastDate = date;
            }

            double mtdDividendPct2 = stockPct * stockDividendMTD + intlPct * intlDividendMTD + bondPct * bondDividendMTD;
            summarySB.AppendLine($"    {lastDate.ToString("MMM", CultureInfo.InvariantCulture)} {lastMTD,7: ##.00;-##.00}%         {mtdDividendPct2: ##.00}%");
            stockDividendYTD += stockDividendMTD;
            intlDividendYTD += intlDividendMTD;
            bondDividendYTD += bondDividendMTD;

            if (year == DateTime.Now.Year && month == DateTime.Now.Month)
            {
                summarySB.Append(daysSection.ToString());
            }

            summarySB.AppendLine("===========================================");

            double finalYtdDiv = stockPct * stockDividendYTD + intlPct * intlDividendYTD + bondPct * bondDividendYTD;

            return new Tuple<double, double>(finalYtd, finalYtdDiv);
        }

        private static Tuple<double, double, double, double, bool, double> calculateDaysPerf(FundValue[] monthlyCloses, Dictionary<DateTime, FundValue> dailyPrices, int index, double lastPrice, DateTime date)
        {
            if (monthlyCloses[index] == null)
            {
                monthlyCloses[index] = new FundValue();
            }

            monthlyCloses[index].Value = dailyPrices[date].Value;

            double ytd = (monthlyCloses[index].Value - monthlyCloses[0].Value) / monthlyCloses[0].Value * 100.0;
            double mtd = (monthlyCloses[index].Value - monthlyCloses[index - 1].Value) / monthlyCloses[index - 1].Value * 100.0;
            double day = (monthlyCloses[index].Value - lastPrice) / lastPrice * 100.0;
            return new Tuple<double, double, double, double, bool, double>(ytd, mtd, day, monthlyCloses[index].Value, dailyPrices[date].Interim, dailyPrices[date].Dividend);
        }
    }
}
