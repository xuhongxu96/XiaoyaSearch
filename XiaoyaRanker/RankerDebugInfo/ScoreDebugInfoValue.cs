using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaRanker.RankerDebugInfo
{
    public class ScoreDebugInfoValue : DebugInfoValue
    {
        public Score Score { get; set; }

        public ScoreDebugInfoValue(Score score)
        {
            Score = score;
        }
    }
}
