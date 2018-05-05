using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaRanker.Ranker
{
    public interface IRanker
    {
        IEnumerable<Score> Rank(IEnumerable<int> urlFileIds, IEnumerable<string> words);
    }
}
