using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRanker.Config;

namespace XiaoyaRanker.QueryTermProximityRanker
{
    public class QueryTermProximityRanker : IRanker
    {
        protected RankerConfig mConfig;

        public QueryTermProximityRanker(RankerConfig config)
        {
            mConfig = config;
        }

        public IEnumerable<double> Rank(IEnumerable<int> urlFileIds, IEnumerable<string> words)
        {
            var wordCount = words.Count();
            var wordTotalLength = words.Sum(o => o.Length);
            var lastWordLength = 0;

            foreach (var id in urlFileIds)
            {
                var wordPositions = new List<List<int>>(wordCount);
                var pointers = new int[wordCount];

                bool skip = false;

                foreach (var word in words)
                {
                    var positions = mConfig.InvertedIndexStore
                        .LoadByWordInUrlFile(id, word)
                        .PositionArr;

                    if (positions.Count == 0)
                    {
                        yield return 0;
                        skip = true;
                        break;
                    }

                    wordPositions.Add(positions);
                    lastWordLength = word.Length;
                }

                if (skip)
                {
                    continue;
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

                    if (wordPosition.Distinct().Count() == wordPosition.Count)
                    {
                        // doesn't have duplicate positions (to deal with the situation that the same word occurs more than one time)

                        var windowLength = wordPosition.Max() - wordPosition.Min();

                        if (minWindowLength == -1 || minWindowLength > windowLength)
                        {
                            minWindowLength = windowLength;
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
                    yield return 0;
                }
                else
                {
                    yield return (double)wordTotalLength / (minWindowLength + lastWordLength);
                }
            }
        }
    }
}
