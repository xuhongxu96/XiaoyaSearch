using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaRetriever.Config;

namespace XiaoyaRetriever.Expression
{
    public abstract class SearchExpression
    {
        public abstract ulong DocumentFrequency { get; }
        public abstract bool IsIncluded { get; }
        public virtual bool IsParsedFromFreeText => false;

        public abstract void SetConfig(RetrieverConfig config);

        public static implicit operator SearchExpression(string w)
        {
            return new Word(w);
        }
    }
}
