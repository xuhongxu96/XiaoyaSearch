using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Store;

namespace XiaoyaRetriever.Config
{
    public class RetrieverConfig
    {
        public IInvertedIndexStore InvertedIndexStore { get; set; }
        public IIndexStatStore IndexStatStore { get; set; }
        public IUrlFileStore UrlFileStore { get; set; }
    }
}
