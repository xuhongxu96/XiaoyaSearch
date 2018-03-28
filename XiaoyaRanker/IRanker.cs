using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaRanker
{
    public interface IRanker
    {
        IEnumerable<double> Rank(IEnumerable<int> urlFileIds, IEnumerable<string> words);
    }
}
