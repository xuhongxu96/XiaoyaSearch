using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaNLP.TextSegmentation;

namespace XiaoyaQueryParser.Config
{
    public class QueryParserConfig
    {
        public ITextSegmenter TextSegmenter { get; set; }
            = new MaxMatchSegmenter("../../../../Resources/30wdict_utf8.txt");
    }
}
