using System;
using System.Collections.Generic;
using System.Text;
using static XiaoyaRanker.PositionRanker.ScoreWithWordPositions;

namespace XiaoyaSearch
{
    public class SearchResult
    {
        public int UrlFileId { get; set; }
        public double Score { get; set; }
        public double ProScore { get; set; }
        public IEnumerable<WordPosition> WordPositions { get; set; }
    }
}
