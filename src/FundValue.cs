using System;
using System.Collections.Generic;

namespace daily
{
    public class FundValues : Dictionary<DateTime, FundValue>
    {
    }

    public class FundValue
    {
        public double Value { get; set; }
        public double Dividend { get; set; }
    }
}
