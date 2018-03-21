using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaRetriever.BooleanRetriever.Expression;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.BooleanRetriever
{
    public class BooleanRetriever
    {
        public IEnumerable<InvertedIndex> Retrieve(IExpression expression)
        {
            if (expression.IsIncluded)
            {
                return expression.Retrieve();
            }
            else
            {
                throw new NotSupportedException("Infinite results to retrieve " +
                    "(Maybe you use NOT to get the complementary set of a finite result)");
            }
        }
    }
} 
