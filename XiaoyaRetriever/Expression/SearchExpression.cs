using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaRetriever.Config;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.Expression
{
    public abstract class SearchExpression
    {
        public abstract long Frequency { get; }
        public abstract bool IsIncluded { get; }
        public virtual bool IsParsedFromFreeText => false;

        public abstract void SetConfig(RetrieverConfig config);

        public static implicit operator SearchExpression(string w)
        {
            return new Word(w);
        }
    }
}
