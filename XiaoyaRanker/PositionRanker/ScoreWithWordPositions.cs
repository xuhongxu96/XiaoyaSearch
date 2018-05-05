using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaRanker.PositionRanker
{
    public class ScoreWithWordPositions : Score
    {
        public class WordPosition
        {
            public string Word { get; set; }
            public int Position { get; set; }
        }
        public IEnumerable<WordPosition> WordPositions { get; set; }
    }
}
