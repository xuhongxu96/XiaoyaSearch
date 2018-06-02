using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRetriever.Config;
using XiaoyaRetriever.Expression;

namespace XiaoyaRetriever.BooleanRetriever
{
    public class BooleanRetriever : IRetriever
    {
        protected RetrieverConfig mConfig;

        public BooleanRetriever(RetrieverConfig config)
        {
            mConfig = config;
        }

        protected IEnumerable<ulong> RetrieveWord(Word word)
        {
            var postingList = mConfig.PostingListStore.GetPostingList(word.Value);
            if (postingList == null)
            {
                return new List<ulong>();
            }
            return from index in postingList.Postings
                   select index.UrlFileId;
        }

        protected IEnumerable<ulong> RetrieveNot(Not notExp)
        {
            return from urlFileID in RetrieveExpression(notExp.Operand)
                   select urlFileID;
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
