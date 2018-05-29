using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaRanker;
using static XiaoyaRanker.PositionRanker.ScoreWithWordPositions;

namespace XiaoyaSearch
{
    public class SearchResult
    {
        public ulong UrlFileId { get; set; }
        public Score Score { get; set; }
        public Score ProScore { get; set; }
        public IEnumerable<WordPosition> WordPositions { get; set; }
    }
}
