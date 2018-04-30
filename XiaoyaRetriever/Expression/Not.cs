using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRetriever.Config;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.Expression
{
    public class Not : SearchExpression
    {
        public SearchExpression Operand { get; private set; }

        protected long mFrequency = 1;
        public override long DocumentFrequency => mFrequency;

        public override bool IsIncluded => !Operand.IsIncluded;

        public Not(SearchExpression operand)
        {
            Operand = operand;
        }

        public override void SetConfig(RetrieverConfig config)
        {
            Operand.SetConfig(config);

            mFrequency = long.MaxValue - Operand.DocumentFrequency;
        }

        public override bool Equals(object obj)
        {
            var not = obj as Not;
            return not != null &&
                   EqualityComparer<SearchExpression>.Default.Equals(Operand, not.Operand);
        }

        public override int GetHashCode()
        {
            return -1936841426 + EqualityComparer<SearchExpression>.Default.GetHashCode(Operand);
        }
    }
}
