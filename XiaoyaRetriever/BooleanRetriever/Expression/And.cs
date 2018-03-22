using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRetriever.Config;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.BooleanRetriever.Expression
{
    public class And : Expression, IEnumerable<Expression>
    {
        protected List<Expression> mOperands = new List<Expression>();

        public override long Frequency => (from o in mOperands select o.Frequency).Min();
        /// <summary>
        /// Not included only when no operand is included.
        /// </summary>
        public override bool IsIncluded => mOperands.Any(o => o.IsIncluded);

        public override IEnumerable<RetrievedUrlFilePositions> Retrieve()
        {
            IEnumerable<RetrievedUrlFilePositions> result = null;

            if (IsIncluded)
            {
                // Someone is included
                foreach (var operand in mOperands.OrderBy(o => o.Frequency))
                {
                    var nextIndices = operand.Retrieve();

                    if (result == null)
                    {
                        result = nextIndices;
                    }
                    else
                    {
                        if (operand.IsIncluded)
                        {
                            result = from a in result
                                     join b in nextIndices on a.UrlFileId equals b.UrlFileId
                                     select new RetrievedUrlFilePositions(a.UrlFileId, a.Union(b));
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
                foreach (var operand in mOperands.OrderByDescending(o => o.Frequency))
                {
                    var nextIndices = operand.Retrieve();

                    if (result == null)
                    {
                        result = nextIndices;
                    }
                    else
                    {
                        var union = from a in result
                                    join b in nextIndices on a.UrlFileId equals b.UrlFileId
                                    select new RetrievedUrlFilePositions(a.UrlFileId, a.Union(b));
                        result = union.Union(result.Union(nextIndices).Except(union));
                    }
                }
            }
            return result;
        }

        public override void SetConfig(RetrieverConfig config)
        {
            foreach (var operand in mOperands)
            {
                operand.SetConfig(config);
            }
        }

        public IEnumerator<Expression> GetEnumerator()
        {
            return mOperands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mOperands.GetEnumerator();
        }

        public void Add(Expression expression)
        {
            mOperands.Add(expression);
        }
    }
}
