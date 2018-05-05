using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaRanker.RankerDebugInfo;

namespace XiaoyaRanker
{
    public class Score
    {
        public double Value { get; set; }
        public DebugInfo DebugInfo { get; set; }

        public override string ToString()
        {
            return string.Format("({0}) {1}", Value, DebugInfo.ToString());
        }
    }
}
