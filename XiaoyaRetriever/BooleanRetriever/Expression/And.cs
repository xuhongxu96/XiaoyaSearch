using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.BooleanRetriever.Expression
{
    public class And : IExpression
    {
        public IEnumerable<IExpression> Operands { get; private set; }

        public long Frequency => (from o in Operands select o.Frequency).Min();
        /// <summary>
        /// Not included only when no operand is included.
        /// </summary>
        public bool IsIncluded => Operands.Any(o => o.IsIncluded);

        public And(IEnumerable<IExpression> operands)
        {
            Operands = operands;
        }

        public IEnumerable<InvertedIndex> Retrieve()
        {
            IEnumerable<InvertedIndex> result = null;

            if (IsIncluded)
            {
                // Someone is included
                foreach (var operand in Operands.OrderBy(o => o.Frequency))
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
                foreach (var operand in Operands.OrderByDescending(o => o.Frequency))
                {
                    var nextIndices = operand.Retrieve();

                    if (result == null)
                    {
                        result = nextIndices;
                    }
                    else
                    {
                        result.Union(nextIndices);
                    }
                }
            }
            return result;
        }
    }
}
