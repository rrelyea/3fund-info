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

        internal double OutputPerfForOneYear(double stock, double intl, double bond, int year, StringBuilder summarySB)
        {
            double bondPct = bond / 100.0;
            double intlPct = stock / 100.0 * intl / 100.0;
            double stockPct = stock / 100.0 * (100 - intl) / 100.0;

            double finalYtd = double.NaN;
            double[] stockClose = new double[13];
            double[] intlClose = new double[13];
            double[] bondClose = new double[13];
            stockClose[0] = double.NaN;

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

            foreach (var date in ThreeFund.StockFund.FundValues[year].Keys)
            {
                month = date.Month;
                if (!storedOpenPrice)
                {
                    stockClose[0] = ThreeFund.StockFund.FundValues[year][date].Value;
                    intlClose[0] = ThreeFund.InternationStockFund.FundValues[year][date].Value;
                    bondClose[0] = ThreeFund.BondFund.FundValues[year][date].Value;
                    lastStockPrice = stockClose[0];
                    lastIntlPrice = intlClose[0];
                    lastBondPrice = bondClose[0];
                    storedOpenPrice = true;
                }

                if (year == date.Year)
                {
                    if (lastMonth < month && month > 1)
                    {
                        summarySB.AppendLine($"              {lastDate.ToString("MMM", CultureInfo.InvariantCulture)} {lastMTD,7: ##.00;-##.00}%");
                    }

                    var stockPerf = calculateDaysPerf(stockClose, ThreeFund.StockFund.FundValues[year], month, lastStockPrice, date);
                    var intlPerf = calculateDaysPerf(intlClose, ThreeFund.InternationStockFund.FundValues[year], month, lastIntlPrice, date);
                    var bondPerf = calculateDaysPerf(bondClose, ThreeFund.BondFund.FundValues[year], month, lastBondPrice, date);

                    lastStockPrice = stockPerf.Item4;
                    lastIntlPrice = intlPerf.Item4;
                    lastBondPrice = bondPerf.Item4;

                    double ytd = stockPct * stockPerf.Item1 + intlPct * intlPerf.Item1 + bondPct * bondPerf.Item1;
                    double mtd = stockPct * stockPerf.Item2 + intlPct * intlPerf.Item2 + bondPct * bondPerf.Item2;
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

                        daysSection.AppendLine($"                            {date.ToString("MMM", CultureInfo.InvariantCulture)} {date.Day:00} {day,7: ##.00;-##.00}%{interimStr}");
                    }

                    lastMTD = mtd;
                    lastMonth = month;
                    finalYtd = ytd;
                }

                lastDate = date;
            }

            summarySB.AppendLine($"              {lastDate.ToString("MMM", CultureInfo.InvariantCulture)} {lastMTD,7: ##.00;-##.00}%");
            if (year == DateTime.Now.Year && month == DateTime.Now.Month)
            {
                summarySB.Append(daysSection.ToString());
            }

            summarySB.AppendLine("===========================================");

            return finalYtd;
        }

        private static Tuple<double, double, double, double, bool> calculateDaysPerf(double[] monthlyCloses, Dictionary<DateTime, FundValue> dailyPrices, int index, double lastPrice, DateTime date)
        {
            monthlyCloses[index] = dailyPrices[date].Value;
            double ytd = (monthlyCloses[index] - monthlyCloses[0]) / monthlyCloses[0] * 100.0;
            double mtd = (monthlyCloses[index] - monthlyCloses[index - 1]) / monthlyCloses[index - 1] * 100.0;
            double day = (monthlyCloses[index] - lastPrice) / lastPrice * 100.0;
            return new Tuple<double, double, double, double, bool>(ytd, mtd, day, monthlyCloses[index], dailyPrices[date].Interim);
        }
    }
}
