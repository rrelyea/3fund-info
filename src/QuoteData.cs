using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace daily
{
    public class QuoteData
    {
        public string Stock { get; private set; }
        public string Intl { get; private set; }
        public string Bond { get; private set; }
        public int Year { get; private set; }

        Dictionary<string, string> stockPrices;
        Dictionary<string, string> intlPrices;
        Dictionary<string, string> bondPrices;

        public QuoteData(string stock, string intl, string bond, int year)
        {
            Stock = stock;
            Intl = intl;
            Bond = bond;
            Year = year;

            stockPrices = LoadData($"prices\\{stock}\\{stock}-{year}.csv");
            intlPrices = LoadData($"prices\\{intl}\\{intl}-{year}.csv");
            bondPrices = LoadData($"prices\\{bond}\\{bond}-{year}.csv");
        }

        internal async Task<double> CalculatePerf(double stock, double intl, double bond, int year, StringBuilder summarySB)
        {
            double bondPct = bond / 100.0;
            double intlPct = stock / 100.0 * intl / 100.0;
            double stockPct = stock / 100.0 * (100 - intl) / 100.0;

            string outputFile = $"perf\\{stock}-{bond} ({intl}% intl)\\{stock}-{bond} ({intl}% intl)-{Year}.csv";
            var outFile = new FileInfo(outputFile);
            if (!outFile.Directory.Exists)
            {
                outFile.Directory.Create();
            }

            double finalYtd = double.NaN;
            int index = 0;
            double[] stockClose = new double[13];
            double[] intlClose = new double[13];
            double[] bondClose = new double[13];
            stockClose[0] = double.NaN;
            StringBuilder sb = new StringBuilder();

            double lastStockPrice = double.NaN;
            double lastIntlPrice = double.NaN;
            double lastBondPrice = double.NaN;
            double lastMTD = double.NaN;
            int lastMonth = 0;
            int month = 0;
            StringBuilder daysSection = new StringBuilder();
            foreach (var date in stockPrices.Keys)
            {
                var currentDate = DateTime.Parse(date);
                month = currentDate.Month;
                if (index == 0)
                {
                    stockClose[index] = double.Parse(stockPrices[date]);
                    intlClose[index] = double.Parse(intlPrices[date]);
                    bondClose[index] = double.Parse(bondPrices[date]);
                    lastStockPrice = stockClose[index];
                    lastIntlPrice = intlClose[index];
                    lastBondPrice = bondClose[index];
                    sb.AppendLine("Date, YTD, MTD, Day");
                }

                index = month;
                if (year == currentDate.Year)
                {
                    if (lastMonth < month && month > 1)
                    {
                        summarySB.AppendLine($"    /{month - 1:00} {lastMTD:0.##}%");
                    }

                    var stockPerf = calculateDaysPerf(stockClose, stockPrices, index, lastStockPrice, date);
                    var intlPerf = calculateDaysPerf(intlClose, intlPrices, index, lastIntlPrice, date);
                    var bondPerf = calculateDaysPerf(bondClose, bondPrices, index, lastBondPrice, date);

                    lastStockPrice = stockPerf.Item4;
                    lastIntlPrice = intlPerf.Item4;
                    lastBondPrice = bondPerf.Item4;

                    double ytd = stockPct * stockPerf.Item1 + intlPct * intlPerf.Item1 + bondPct * bondPerf.Item1;
                    double mtd = stockPct * stockPerf.Item2 + intlPct * intlPerf.Item2 + bondPct * bondPerf.Item2;
                    double day = stockPct * stockPerf.Item3 + intlPct * intlPerf.Item3 + bondPct * bondPerf.Item3;
                    sb.AppendLine($"{date}, {ytd:0.##}%, {mtd:0.##}%, {day:0.##}%");
                    if (year == DateTime.Now.Year && month == DateTime.Now.Month)
                    {
                        daysSection.AppendLine($"               {currentDate.Month:00}/{currentDate.Day:00} {day:0.##}%");
                    }

                    lastMTD = mtd;
                    lastMonth = month;
                    finalYtd = ytd;
                }
            }

            summarySB.AppendLine($"    /{month:00} {lastMTD:0.##}%");
            if (year == DateTime.Now.Year && month == DateTime.Now.Month)
            {
                summarySB.Append(daysSection.ToString());
            }

            // Don't create yearly perf files.
            // File.WriteAllText(outputFile, sb.ToString());
            return finalYtd;
        }

        private static Tuple<double, double, double, double> calculateDaysPerf(double[] monthlyCloses, Dictionary<string, string> dailyPrices, int index, double lastPrice, string date)
        {
            monthlyCloses[index] = double.Parse(dailyPrices[date]);
            double ytd = (monthlyCloses[index] - monthlyCloses[0]) / monthlyCloses[0] * 100.0;
            double mtd = (monthlyCloses[index] - monthlyCloses[index - 1]) / monthlyCloses[index - 1] * 100.0;
            double day = (monthlyCloses[index] - lastPrice) / lastPrice * 100.0;
            return new Tuple<double, double, double, double>(ytd, mtd, day, monthlyCloses[index]);
        }
        private Dictionary<string, string> LoadData(string fileName)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            bool skipNextLine = true;

            foreach (var line in File.ReadAllLines(fileName))
            {
                if (skipNextLine)
                {
                    skipNextLine = false;
                    continue;
                }

                var chunks = line.Split(',');
                data.Add(chunks[0], chunks[1].Substring(1));
            }

            return data;
        }

    }
}
