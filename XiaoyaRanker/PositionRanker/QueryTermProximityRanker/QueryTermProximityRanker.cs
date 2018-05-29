using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRanker.Config;
using XiaoyaRanker.RankerDebugInfo;

namespace XiaoyaRanker.PositionRanker.QueryTermProximityRanker
{
    public class QueryTermProximityRanker : IPositionRanker
    {
        protected RankerConfig mConfig;

        public QueryTermProximityRanker(RankerConfig config)
        {
            mConfig = config;
        }

        public IEnumerable<ScoreWithWordPositions> Rank(IEnumerable<ulong> urlFileIds, IEnumerable<string> words)
        {
            var wordList = words.ToList();
            var wordCount = wordList.Count;

            foreach (var id in urlFileIds)
            {
                var lastWordLength = 0;
                var wordTotalLength = 0;
                var notExistedWordCount = 0;

                var wordPositions = new List<List<uint>>(wordCount);
                List<uint> bestWordPosition = null;
                var pointers = new int[wordCount];

                foreach (var word in wordList)
                {
                    var positions = mConfig.InvertedIndexStore
                        .GetIndex(id, word)?
                        .Positions;

                    if (positions == null || positions.Count == 0)
                    {
                        wordPositions.Add(new List<uint> { uint.MaxValue });
                        notExistedWordCount++;
                    }
                    else
                    {
                        wordPositions.Add(positions.ToList());
                        wordTotalLength += word.Length;
                    }
                }

                uint minWindowLength = uint.MaxValue;
                bool isAllEnd = false;
                while (!isAllEnd)
                {
                    var wordPosition = new List<uint>(wordCount);

                    for (int i = 0; i < wordCount; ++i)
                    {
                        wordPosition.Add(wordPositions[i][pointers[i]]);
                    }

                    var validWordPosition = wordPosition.Where(o => o != uint.MaxValue).ToList();

                    if (validWordPosition.Distinct().Count() == validWordPosition.Count
                        && validWordPosition.Count > 0)
                    {
                        // doesn't have duplicate positions (to deal with the situation that the same word occurs more than one time)

                        var windowLength = validWordPosition.Max() - validWordPosition.Min();

                        if (minWindowLength == uint.MaxValue || minWindowLength > windowLength)
                        {
                            // update window length
                            minWindowLength = windowLength;
                            bestWordPosition = wordPosition;
                            lastWordLength = wordList[wordPosition.IndexOf(validWordPosition.Max())].Length;
                        }
                    }

                    var movePointerIndex = -1;

                    for (int i = 0; i < wordCount; ++i)
                    {
                        var currentPointer = pointers[i];
                        if (currentPointer + 1 < wordPositions[i].Count
                            && (movePointerIndex == -1
                            || wordPositions[i][currentPointer] < wordPositions[movePointerIndex][pointers[movePointerIndex]]))
                        {
                            movePointerIndex = i;
                        }
                    }

                    if (movePointerIndex == -1)
                    {
                        isAllEnd = true;
                    }
                    else
                    {
                        pointers[movePointerIndex]++;
                    }
                }

                if (minWindowLength == uint.MaxValue)
                {
                    yield return new ScoreWithWordPositions
                    {
                        Value = 0,
                        DebugInfo = new DebugInfo(nameof(QueryTermProximityRanker),
                            "Error", "No Word Was Found"),
                        WordPositions = null,
                    };
                }
                else
                {
                    var debugInfo = new DebugInfo(nameof(QueryTermProximityRanker));
                    debugInfo.Properties["MinWindowLength"] = new StringDebugInfoValue(minWindowLength.ToString());
                    debugInfo.Properties["LastWordLength"] = new StringDebugInfoValue(lastWordLength.ToString());
                    debugInfo.Properties["WordTotalLength"] = new StringDebugInfoValue(wordTotalLength.ToString());
                    debugInfo.Properties["NotExistedWordCount"] = new StringDebugInfoValue(notExistedWordCount.ToString());

                    yield return new ScoreWithWordPositions
                    {
                        Value = (double)wordTotalLength / (minWindowLength + lastWordLength) / (1 + notExistedWordCount),
                        DebugInfo = debugInfo,
                        WordPositions = bestWordPosition.Zip(wordList, (p, w) => new ScoreWithWordPositions.WordPosition
                        {
                            Word = w,
                            Position = p,
                        }),
                    };
                }
            }
        }
    }
}
