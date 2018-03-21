using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.BooleanRetriever.Expression
{
    public class Or : IExpression
    {
        public IEnumerable<IExpression> Operands { get; private set; }

        public long Frequency => (from o in Operands select o.Frequency).Max();
        /// <summary>
        /// Included only when all operands are included.
        /// </summary>
        public bool IsIncluded => !Operands.Any(o => !o.IsIncluded);

        public Or(IEnumerable<IExpression> operands)
        {
            Operands = operands;
        }

        public IEnumerable<InvertedIndex> Retrieve()
        {
            IEnumerable<InvertedIndex> result = null;

            if (IsIncluded)
            {
                // All are included
                foreach (var operand in Operands.OrderBy(o => o.Frequency))
                {
                    var nextIndices = operand.Retrieve();

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
                foreach (var operand in Operands.OrderByDescending(o => o.Frequency))
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
                            result = result.Intersect(nextIndices);
                        }
                    }
                }
            }
            return result;
        }
    }
}
