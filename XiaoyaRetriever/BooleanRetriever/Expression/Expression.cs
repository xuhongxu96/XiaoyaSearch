using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaRetriever.Config;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.BooleanRetriever.Expression
{
    public abstract class Expression
    {
        public abstract long Frequency { get; }
        public abstract bool IsIncluded { get; }

        public abstract void SetConfig(RetrieverConfig config);
        public abstract IEnumerable<RetrievedUrlFilePositions> Retrieve();

        public static implicit operator Expression(string w)
        {
            return new Word(w);
        }
    }
}
