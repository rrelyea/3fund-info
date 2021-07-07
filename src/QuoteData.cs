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

        Dictionary<string, string> stockPrices;
        Dictionary<string, string> intlPrices;
        Dictionary<string, string> bondPrices;

        public QuoteData(string stock, string intl, string bond, int year)
        {
            Stock = stock;
            Intl = intl;
            Bond = bond;

            stockPrices = LoadData($"prices\\vanguard\\{stock}\\{stock}-{year}.csv");
            intlPrices = LoadData($"prices\\vanguard\\{intl}\\{intl}-{year}.csv");
            bondPrices = LoadData($"prices\\vanguard\\{bond}\\{bond}-{year}.csv");
        }

        internal Task CalculatePerf(int stock, int intl, int bond)
        {
            throw new NotImplementedException();
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
