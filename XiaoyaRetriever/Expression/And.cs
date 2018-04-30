using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaRetriever.Config;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.Expression
{
    public class And : SearchExpression, IEnumerable<SearchExpression>
    {
        protected List<SearchExpression> mOperands = new List<SearchExpression>();

        protected long mFrequency = 1;
        public override long DocumentFrequency => mFrequency;
        /// <summary>
        /// Not included only when no operand is included.
        /// </summary>
        public override bool IsIncluded => mOperands.Any(o => o.IsIncluded);

        public override void SetConfig(RetrieverConfig config)
        {
            if (!mOperands.Any())
            {
                return;
            }

            foreach (var operand in mOperands)
            {
                operand.SetConfig(config);
            }

            if (mOperands.Any())
            {
                mFrequency = (from o in mOperands select o.DocumentFrequency).Min();
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
            var and = obj as And;
            return and != null &&
                   EqualityComparer<List<SearchExpression>>.Default.Equals(mOperands, and.mOperands);
        }

        public override int GetHashCode()
        {
            return 1857792354 + EqualityComparer<List<SearchExpression>>.Default.GetHashCode(mOperands);
        }

        public override bool IsParsedFromFreeText
        {
            get
            {
                foreach (var operand in mOperands)
                {
                    if (operand is Word) continue;
                    if (operand is Not notExp
                        && notExp.Operand is And andExp)
                    {
                        return andExp.All(o => o is Word);
                    }
                    return false;
                }
                return true;
            }
        }
    }
}
