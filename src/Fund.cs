using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace daily
{
    public class Fund
    {
        public Fund(string symbol)
        {
            Symbol = symbol;
        }

        public string Symbol { get; private set; }
        public string UpperSymbol { get { return Symbol.ToUpper(); } }

        private Dictionary<DateTime, FundValue> _fundValues;
        public Dictionary<DateTime, FundValue> FundValues
        {
            get
            { 
                if (_fundValues == null)
                {
                    _fundValues = new Dictionary<DateTime, FundValue>();
                }

                return _fundValues;
            }
        }
    }
}
