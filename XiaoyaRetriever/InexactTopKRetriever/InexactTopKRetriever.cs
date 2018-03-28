using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaCommon.Helper;
using XiaoyaRetriever.Config;
using XiaoyaRetriever.Expression;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.InexactTopKRetriever
{
    public class InexactTopKRetriever : IRetriever
    {
        protected readonly int[] WORD_FREQUENCY_THRESHOLDS_FOR_TIERS = new int[] { int.MaxValue, 20, 10, 2, 1 };

        protected RetrieverConfig mConfig;
        protected int mTopK;

        public InexactTopKRetriever(RetrieverConfig config, int k = 1000)
        {
            mConfig = config;
            mTopK = k;
        }

        protected IEnumerable<UrlFileIndexStat> RetrieveWord(Word word)
        {
            return (from index in mConfig.UrlFileIndexStatStore.LoadByWord(word.Value)
                    select index);
        }

        protected IEnumerable<int> RetrieveNot(Not not)
        {
            IEnumerable<int> result = null;
            foreach (var word in not.Operand as And)
            {
                var nextIndices = from index in mConfig.UrlFileIndexStatStore.LoadByWord((word as Word).Value)
                                  select index.UrlFileId;
                if (result == null)
                {
                    result = nextIndices;
                }
                else
                {
                    result.Intersect(nextIndices);
                }
            }
            return result;
        }

        protected IEnumerable<int> RetrieveExpression(SearchExpression expression)
        {
            if (expression is And andExp)
            {
                int count = 0;
                var urlFilesForWords = new Dictionary<Word, IEnumerable<UrlFileIndexStat>>();

                var documentCount = mConfig.UrlFileStore.Count();

                var sortedWordExps = (from exp in andExp
                                      where exp is Word
                                      select exp as Word).OrderByDescending(o =>
                         ScoringHelpers.TfIdf(o.Frequency, o.DocumentFrequency, documentCount)).ToList();
                var notWordExps = (from exp in andExp
                                   where exp is Not
                                   select exp as Not).OrderByDescending(o => o.Frequency).ToList();

                foreach (var word in sortedWordExps)
                {
                    urlFilesForWords[word] = RetrieveWord(word);
                }

                for (int tier = 1; tier < WORD_FREQUENCY_THRESHOLDS_FOR_TIERS.Length; ++tier)
                {
                    IEnumerable<int> result = null;
                    bool isFirstWord = true;

                    foreach (var word in sortedWordExps)
                    {
                        int curTier = tier;
                        var nextIndices = urlFilesForWords[word]
                                        .Where(o => o.WordFrequency >= WORD_FREQUENCY_THRESHOLDS_FOR_TIERS[curTier]
                                                && o.WordFrequency < WORD_FREQUENCY_THRESHOLDS_FOR_TIERS[curTier - 1])
                                        .Select(o => o.UrlFileId);

                        if (isFirstWord)
                        {
                            if (result == null)
                            {
                                result = nextIndices;
                            }
                            else
                            {
                                result = result.Union(nextIndices);
                            }
                            isFirstWord = false;
                        }
                        else
                        {
                            result = result.Intersect(nextIndices);
                        }
                    }

                    foreach (var notWord in notWordExps)
                    {
                        result = result.Except(RetrieveNot(notWord));
                    }

                    foreach (var urlFileId in result)
                    {
                        yield return urlFileId;
                        ++count;

                        if (count >= mTopK)
                        {
                            yield break;
                        }
                    }
                }
            }
            else
            {
                throw new NotSupportedException("Not supported search expression");
            }
        }

        /// <summary>
        /// Retrieve UrlFiles by SearchExpression
        /// </summary>
        /// <param name="expression">Search expression</param>
        /// <returns>An enumerable of UrlFile IDs</returns>
        public IEnumerable<int> Retrieve(SearchExpression expression)
        {
            if (!expression.IsParsedFromFreeText)
            {
                throw new NotSupportedException("Not supported search expression");
            }

            expression.SetConfig(mConfig);

            if (expression.IsIncluded)
            {
                return RetrieveExpression(expression);
            }
            else
            {
                throw new NotSupportedException("Infinite results to retrieve " +
                    "(Maybe you use NOT to get the complementary set of a finite result)");
            }
        }
    }
}
