using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaRetriever.Expression;

namespace XiaoyaQueryParser.QueryParser
{
    public class ParsedQuery
    {
        public SearchExpression Expression { get; set; }
        public IEnumerable<string> Words { get; set; }
    }
}
