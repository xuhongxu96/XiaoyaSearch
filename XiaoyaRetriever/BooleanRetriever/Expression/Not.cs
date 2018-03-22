using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRetriever.Config;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.BooleanRetriever.Expression
{
    public class Not : Expression
    {
        public Expression Operand { get; private set; }
        public override long Frequency => long.MaxValue - Operand.Frequency;
        public override bool IsIncluded => !Operand.IsIncluded;

        public Not(Expression operand)
        {
            Operand = operand;
        }

        public override IEnumerable<RetrievedUrlFilePositions> Retrieve()
        {
            return from position in Operand.Retrieve()
                   select position;
        }

        public override void SetConfig(RetrieverConfig config)
        {
            Operand.SetConfig(config);
        }
    }
}
