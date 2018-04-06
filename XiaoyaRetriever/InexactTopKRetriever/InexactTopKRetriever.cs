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
        protected readonly int[] WORD_FREQUENCY_THRESHOLDS_FOR_TIERS = new int[] { int.MaxValue / 2, 20, 10, 2, 1 };

        protected RetrieverConfig mConfig;
        protected int mTopK;
        protected Dictionary<string, IEnumerable<int>> mWordCaches = new Dictionary<string, IEnumerable<int>>();

        public InexactTopKRetriever(RetrieverConfig config, int k = 1000)
        {
            mConfig = config;
            mTopK = k;
        }

        protected IEnumerable<int> RetrieveWord(Word word, int minFrequency, int maxFrequency)
        {
            var result = from index in mConfig.UrlFileIndexStatStore.LoadByWord(word.Value)
                         where index.WordFrequency >= minFrequency && index.WordFrequency < maxFrequency
                         select index.UrlFileId;
            if (mWordCaches.ContainsKey(word.Value))
            {
                result = result.Union(mWordCaches[word.Value]);
            }
            mWordCaches[word.Value] = result;
            return result;
        }

        protected IEnumerable<int> RetrieveNot(Not notExp, int minFrequency, int maxFrequency)
        {
            return from position in
                       RetrieveExpression(notExp.Operand, -minFrequency, int.MaxValue - maxFrequency)
                   select position;
        }

        protected IEnumerable<int> RetrieveAnd(And andExp, int minFrequency, int maxFrequency)
        {
            IEnumerable<int> result = null;

            if (andExp.IsIncluded)
            {
                // Someone is included
                foreach (var operand in andExp.OrderBy(o => o.Frequency))
                {
                    var nextIndices = RetrieveExpression(operand, minFrequency, maxFrequency);

                    if (result == null)
                    {
                        result = nextIndices;
                    }
                    else
                    {
                        if (operand.IsIncluded)
                        {
                            result = result.Intersect(nextIndices);
                        }
                        else
                        {
                            result = result.Except(nextIndices);
                        }
                    }
                }
            }
            else
            {
                // None is included
                foreach (var operand in andExp.OrderByDescending(o => o.Frequency))
                {
                    var nextIndices = RetrieveExpression(operand, minFrequency, maxFrequency);

                    if (result == null)
                    {
                        result = nextIndices;
                    }
                    else
                    {
                        result = result.Union(nextIndices);
                    }
                }
            }
            return result;
        }


        protected IEnumerable<int> RetrieveOr(Or orExp, int minFrequency, int maxFrequency)
        {
            IEnumerable<int> result = null;

            if (orExp.IsIncluded)
            {
                // All are included
                foreach (var operand in orExp.OrderBy(o => o.Frequency))
                {
                    var nextIndices = RetrieveExpression(operand, minFrequency, maxFrequency);

                    if (result == null)
                    {
                        result = nextIndices;
                    }
                    else
                    {
                        result = result.Union(nextIndices);
                    }
                }
            }
            else
            {
                // Someone is not included
                foreach (var operand in orExp.OrderByDescending(o => o.Frequency))
                {
                    var nextIndices = RetrieveExpression(operand, minFrequency, maxFrequency);

                    if (result == null)
                    {
                        result = nextIndices;
                    }
                    else
                    {
                        if (operand.IsIncluded)
                        {
                            result = result.Except(nextIndices);
                        }
                        else
                        {
                            result = result.Intersect(nextIndices);
                        }
                    }
                }
            }
            return result;
        }

        protected IEnumerable<int> RetrieveExpression(SearchExpression expression, int minFrequency, int maxFrequency)
        {
            if (expression is Word word)
            {
                return RetrieveWord(word, minFrequency, maxFrequency);
            }
            else if (expression is Not notExp)
            {
                return RetrieveNot(notExp, minFrequency, maxFrequency);
            }
            else if (expression is And andExp)
            {
                return RetrieveAnd(andExp, minFrequency, maxFrequency);
            }
            else if (expression is Or orExp)
            {
                return RetrieveOr(orExp, minFrequency, maxFrequency);
            }
            throw new NotSupportedException("Not supported search expression");
        }

        protected IEnumerable<int> RetrieveExpression(SearchExpression expression)
        {
            int count = 0;

            mWordCaches.Clear();

            var existedResult = new HashSet<int>();

            for (int tier = 1; tier < WORD_FREQUENCY_THRESHOLDS_FOR_TIERS.Length; ++tier)
            {
                var result = RetrieveExpression(expression,
                    WORD_FREQUENCY_THRESHOLDS_FOR_TIERS[tier],
                    WORD_FREQUENCY_THRESHOLDS_FOR_TIERS[tier - 1]);

                foreach (var urlFileId in result)
                {
                    if (existedResult.Contains(urlFileId))
                    {
                        continue;
                    }
                    existedResult.Add(urlFileId);

                    yield return urlFileId;
                    ++count;

                    if (count >= mTopK)
                    {
                        yield break;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve UrlFiles by SearchExpression
        /// </summary>
        /// <param name="expression">Search expression</param>
        /// <returns>An enumerable of UrlFile IDs</returns>
        public IEnumerable<int> Retrieve(SearchExpression expression)
        {
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
