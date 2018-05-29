using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRetriever.Config;

namespace XiaoyaRetriever.Expression
{
    public class Not : SearchExpression
    {
        public SearchExpression Operand { get; private set; }

        protected ulong mFrequency = 1;
        public override ulong DocumentFrequency => mFrequency;

        public override bool IsIncluded => !Operand.IsIncluded;

        public Not(SearchExpression operand)
        {
            Operand = operand;
        }

        public override void SetConfig(RetrieverConfig config)
        {
            Operand.SetConfig(config);

            mFrequency = ulong.MaxValue - Operand.DocumentFrequency;
        }

        public override bool Equals(object obj)
        {
            return obj is Not not &&
                   EqualityComparer<SearchExpression>.Default.Equals(Operand, not.Operand);
        }

        public override int GetHashCode()
        {
            return -1936841426 + EqualityComparer<SearchExpression>.Default.GetHashCode(Operand);
        }
    }
}
