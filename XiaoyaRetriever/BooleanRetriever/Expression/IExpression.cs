using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever.BooleanRetriever.Expression
{
    public interface IExpression
    {
        long Frequency { get; }
        bool IsIncluded { get; }
        IEnumerable<InvertedIndex> Retrieve();
    }
}
