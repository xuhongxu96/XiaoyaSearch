using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRetriever.Config;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.BooleanRetriever.Expression
{
    public class Or : Expression, IEnumerable<Expression>
    {
        protected List<Expression> mOperands = new List<Expression>();

        public override long Frequency => (from o in mOperands select o.Frequency).Max();
        /// <summary>
        /// Included only when all operands are included.
        /// </summary>
        public override bool IsIncluded => !mOperands.Any(o => !o.IsIncluded);

        public override IEnumerable<RetrievedUrlFilePositions> Retrieve()
        {
            IEnumerable<RetrievedUrlFilePositions> result = null;

            if (IsIncluded)
            {
                // All are included
                foreach (var operand in mOperands.OrderBy(o => o.Frequency))
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
            else
            {
                // Someone is not included
                foreach (var operand in mOperands.OrderByDescending(o => o.Frequency))
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
                            result = result.Except(nextIndices);
                        }
                        else
                        {
                            result = from a in result
                                     join b in nextIndices on a.UrlFileId equals b.UrlFileId
                                     select new RetrievedUrlFilePositions(a.UrlFileId, a.Union(b));
                        }
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
