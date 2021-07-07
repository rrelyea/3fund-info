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
        Dictionary<string, string> stockPrices = new Dictionary<string, string>();
        Dictionary<string, string> intlPrices = new Dictionary<string, string>();
        Dictionary<string, string> bondPrices = new Dictionary<string, string>();

        public QuoteData(string stock, string intl, string bond, int year)
        {
            Stock = stock;
            Intl = intl;
            Bond = bond;

            bool skipNextLine = true;

            foreach (var line in File.ReadAllLines($"prices\\vanguard\\{stock}-{year}.csv"))
            {
                if (skipNextLine)
                {
                    skipNextLine = false;
                    continue;
                }

                var chunks = line.Split(',');
                stockPrices.Add(chunks[0], chunks[1].Substring(1));
            }
        }


    }
}
