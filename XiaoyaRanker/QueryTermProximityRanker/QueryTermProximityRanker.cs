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
            var wordPositions = new List<List<int>>(wordCount);
            var pointers = new int[wordCount];

            foreach (var id in urlFileIds)
            {
                foreach (var word in words)
                {
                    wordPositions.Add(mConfig.InvertedIndexStore
                        .LoadByWordInUrlFileOrderByPosition(id, word)
                        .Select(o => o.Position)
                        .ToList());
                }

                for (int i = 0; i < wordCount; ++i)
                {
                    pointers[i] = 0;
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

                    var windowLength = wordPosition.Max() - wordPosition.Min();

                    if (minWindowLength == -1 || minWindowLength > windowLength)
                    {
                        minWindowLength = windowLength;
                    }

                    var movePointerIndex = -1;

                    for (int i = 0; i < wordCount; ++i)
                    {
                        var currentPointer = pointers[i];
                        if (currentPointer + 1 < wordPositions[i].Count
                            && (movePointerIndex == -1 || currentPointer < pointers[movePointerIndex]))
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

                yield return minWindowLength;
            }
        }
    }
}
