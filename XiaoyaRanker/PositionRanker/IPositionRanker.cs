using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaRanker.PositionRanker
{
    public interface IPositionRanker
    {
        IEnumerable<ScoreWithWordPositions> Rank(IEnumerable<ulong> urlFileIds, IEnumerable<string> words);
    }
   
}
