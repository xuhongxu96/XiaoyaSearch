using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.BooleanRetriever.Expression
{
    public class Not : IExpression
    {
        public IExpression Operand { get; private set; }
        public long Frequency => long.MaxValue - Operand.Frequency;
        public bool IsIncluded => !Operand.IsIncluded;

        public Not(IExpression operand)
        {
            Operand = operand;
        }

        public IEnumerable<InvertedIndex> Retrieve()
        {
            return from index in Operand.Retrieve()
                   select index;
        }
    }
}
