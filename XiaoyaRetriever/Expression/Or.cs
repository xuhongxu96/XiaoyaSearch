using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRetriever.Config;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.Expression
{
    public class Or : SearchExpression, IEnumerable<SearchExpression>
    {
        protected List<SearchExpression> mOperands = new List<SearchExpression>();

        protected long mFrequency = 1;
        public override long Frequency => mFrequency;
        /// <summary>
        /// Included only when all operands are included.
        /// </summary>
        public override bool IsIncluded => !mOperands.Any(o => !o.IsIncluded);

        public override void SetConfig(RetrieverConfig config)
        {
            foreach (var operand in mOperands)
            {
                operand.SetConfig(config);
            }

            if (mOperands.Any())
            {
                mFrequency = (from o in mOperands select o.Frequency).Max();
            }
            else
            {
                mFrequency = 0;
            }
        }

        public IEnumerator<SearchExpression> GetEnumerator()
        {
            return mOperands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mOperands.GetEnumerator();
        }

        public void Add(SearchExpression expression)
        {
            mOperands.Add(expression);
        }

        public override bool Equals(object obj)
        {
            var or = obj as Or;
            return or != null &&
                   EqualityComparer<List<SearchExpression>>.Default.Equals(mOperands, or.mOperands);
        }

        public override int GetHashCode()
        {
            return 1857792354 + EqualityComparer<List<SearchExpression>>.Default.GetHashCode(mOperands);
        }
    }
}
