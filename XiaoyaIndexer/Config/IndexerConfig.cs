using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Store;

namespace XiaoyaIndexer.Config
{
    public class IndexerConfig
    {
        public IUrlFileStore UrlFileStore { get; set; }
        public IInvertedIndexStore InvertedIndexStore { get; set; }
    }
}
