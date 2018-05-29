using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaCommon.Helper;
using XiaoyaRetriever.Config;
using XiaoyaRetriever.Expression;
using XiaoyaStore.Cache;
using XiaoyaStore.Helper;

namespace XiaoyaRetriever.InexactTopKRetriever
{
    public class InexactTopKRetriever : IRetriever
    {
        // protected readonly double[] WORD_WEIGHT_THRESHOLDS_FOR_TIERS = new double[] { double.MaxValue / 2, 2.5, 2.1, 1, 0 };

        protected RetrieverConfig mConfig;
        protected int mTopK;

        public InexactTopKRetriever(RetrieverConfig config, int k = 1000)
        {
            mConfig = config;
            mTopK = k;
        }

        protected IEnumerable<ulong> RetrieveWord(Word word)
        {
            return mConfig.PostingListStore.GetPostingList(word.Value).Postings
                    .OrderByDescending(id => mConfig.InvertedIndexStore.GetIndex(id, word.Value).Weight)
                    .Take(mTopK);

        }

        protected IEnumerable<ulong> RetrieveNot(Not notExp)
        {
            return from position in RetrieveExpression(notExp.Operand)
                   select position;
        }

        protected IEnumerable<ulong> RetrieveAnd(And andExp)
        {
            IEnumerable<ulong> result = null;

            if (andExp.IsIncluded)
            {
                // Someone is included
                foreach (var operand in andExp.OrderBy(o => o.DocumentFrequency))
                {
                    var nextIndices = RetrieveExpression(operand);

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
                foreach (var operand in andExp.OrderByDescending(o => o.DocumentFrequency))
                {
                    var nextIndices = RetrieveExpression(operand);

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


        protected IEnumerable<ulong> RetrieveOr(Or orExp)
        {
            IEnumerable<ulong> result = null;

            if (orExp.IsIncluded)
            {
                // All are included
                foreach (var operand in orExp.OrderBy(o => o.DocumentFrequency))
                {
                    var nextIndices = RetrieveExpression(operand);

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
                foreach (var operand in orExp.OrderByDescending(o => o.DocumentFrequency))
                {
                    var nextIndices = RetrieveExpression(operand);

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

        protected IEnumerable<ulong> RetrieveExpression(SearchExpression expression)
        {
            if (expression is Word word)
            {
                return RetrieveWord(word);
            }
            else if (expression is Not notExp)
            {
                return RetrieveNot(notExp);
            }
            else if (expression is And andExp)
            {
                return RetrieveAnd(andExp);
            }
            else if (expression is Or orExp)
            {
                return RetrieveOr(orExp);
            }
            throw new NotSupportedException("Not supported search expression");
        }

        /// <summary>
        /// Retrieve UrlFiles by SearchExpression
        /// </summary>
        /// <param name="expression">Search expression</param>
        /// <returns>An enumerable of UrlFile IDs</returns>
        public IEnumerable<ulong> Retrieve(SearchExpression expression)
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
