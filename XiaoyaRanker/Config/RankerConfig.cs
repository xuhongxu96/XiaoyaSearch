using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Store;

namespace XiaoyaRanker.Config
{
    public class RankerConfig
    {
        public UrlFileStore UrlFileStore { get; set; }
        public UrlFileIndexStatStore UrlFileIndexStatStore { get; set; }
        public IndexStatStore IndexStatStore { get; set; }
        public InvertedIndexStore InvertedIndexStore { get; set; }
    }
}
