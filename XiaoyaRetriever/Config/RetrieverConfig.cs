using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Store;

namespace XiaoyaRetriever.Config
{
    public class RetrieverConfig
    {
        public InvertedIndexStore InvertedIndexStore { get; set; }
        public IndexStatStore IndexStatStore { get; set; }
    }
}
