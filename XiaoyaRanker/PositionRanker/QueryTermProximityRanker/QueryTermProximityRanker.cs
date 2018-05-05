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

        public IEnumerable<ScoreWithWordPositions> Rank(IEnumerable<int> urlFileIds, IEnumerable<string> words)
        {
            var wordList = words.ToList();
            var wordCount = wordList.Count;

            foreach (var id in urlFileIds)
            {
                var lastWordLength = 0;
                var wordTotalLength = 0;
                var notExistedWordCount = 0;

                var wordPositions = new List<List<int>>(wordCount);
                List<int> bestWordPosition = null;
                var pointers = new int[wordCount];

                foreach (var word in wordList)
                {
                    var positions = mConfig.InvertedIndexStore
                        .LoadByWordInUrlFile(id, word)?
                        .PositionArr;

                    if (positions == null || positions.Count == 0)
                    {
                        wordPositions.Add(new List<int> { -1 });
                        notExistedWordCount++;
                    }
                    else
                    {
                        wordPositions.Add(positions);
                        wordTotalLength += word.Length;
                    }
                }

                var minWindowLength = -1;
                bool isAllEnd = false;
                while (!isAllEnd)
                {
                    var wordPosition = new List<int>(wordCount);

                    for (int i = 0; i < wordCount; ++i)
                    {
                        wordPosition.Add(wordPositions[i][pointers[i]]);
                    }

                    var validWordPosition = wordPosition.Where(o => o != -1).ToList();

                    if (validWordPosition.Distinct().Count() == validWordPosition.Count
                        && validWordPosition.Count > 0)
                    {
                        // doesn't have duplicate positions (to deal with the situation that the same word occurs more than one time)

                        var windowLength = validWordPosition.Max() - validWordPosition.Min();

                        if (minWindowLength == -1 || minWindowLength > windowLength)
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

                if (minWindowLength == -1)
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
