﻿using System.Collections.Generic;

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

        private Dictionary<int, FundValues> _fundValues;
        public Dictionary<int, FundValues> FundValues
        {
            get
            { 
                if (_fundValues == null)
                {
                    _fundValues = new Dictionary<int, FundValues>();
                }

                return _fundValues;
            }
        }
    }
}
