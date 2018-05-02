using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaNLP.TextSegmentation;

namespace XiaoyaQueryParser.Config
{
    public class QueryParserConfig
    {
        public ITextSegmenter TextSegmenter { get; set; }
            = new JiebaSegmenter(false);
    }
}
