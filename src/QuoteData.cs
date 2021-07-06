using System;
using System.Collections.Generic;
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
        public QuoteData(string stock, string intl, string bond)
        {
            Stock = stock;
            Intl = intl;
            Bond = bond;
        }
    }
}
