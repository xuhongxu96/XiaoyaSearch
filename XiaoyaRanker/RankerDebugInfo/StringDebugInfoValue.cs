using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaRanker.RankerDebugInfo
{
    public class StringDebugInfoValue : DebugInfoValue
    {
        public string Value { get; set; }

        public StringDebugInfoValue(string value)
        {
            Value = value;
        }
    }
}
