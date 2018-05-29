using System.Collections.Generic;
using XiaoyaRetriever.Expression;

namespace XiaoyaRetriever
{
    public interface IRetriever
    {
        IEnumerable<ulong> Retrieve(SearchExpression expression);
    }
}