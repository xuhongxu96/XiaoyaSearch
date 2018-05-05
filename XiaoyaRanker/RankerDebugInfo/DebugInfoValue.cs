using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaRanker.RankerDebugInfo
{
    public class DebugInfoValue
    {
        public override string ToString()
        {
            if (this is StringDebugInfoValue strVal)
            {
                return strVal.Value;
            }
            else if (this is ScoreDebugInfoValue scoreVal)
            {
                return scoreVal.Score.ToString();
            }
            return "";
        }
    }
}
