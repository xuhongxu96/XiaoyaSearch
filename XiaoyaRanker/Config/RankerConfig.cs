using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Store;

namespace XiaoyaRanker.Config
{
    public class RankerConfig
    {
        public IUrlFileStore UrlFileStore { get; set; }
        public IUrlFileIndexStatStore UrlFileIndexStatStore { get; set; }
        public IIndexStatStore IndexStatStore { get; set; }
        public IInvertedIndexStore InvertedIndexStore { get; set; }
    }
}
