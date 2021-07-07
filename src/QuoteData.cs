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

            stockPrices = LoadData($"prices\\vanguard\\{stock}\\{stock}-{year}.csv");
            intlPrices = LoadData($"prices\\vanguard\\{intl}\\{intl}-{year}.csv");
            bondPrices = LoadData($"prices\\vanguard\\{bond}\\{bond}-{year}.csv");
        }

        internal async Task CalculatePerf(int stock, int intl, int bond)
        {
            string outputFile = $"perf\\vanguard\\us {stock}-bond {bond}-intl {intl}\\us {stock}-bond {bond}-intl {intl}-{Year}.csv";

            return;
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
