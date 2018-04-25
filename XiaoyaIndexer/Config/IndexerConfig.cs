using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Store;

namespace XiaoyaIndexer.Config
{
    public class IndexerConfig
    {
        public string LogDirectory { get; set; } = "Logs";

        public int MaxIndexingConcurrency { get; set; } = 30;

        public IUrlFileStore UrlFileStore { get; set; }
        public IInvertedIndexStore InvertedIndexStore { get; set; }
        public ILinkStore LinkStore { get; set; }
        public IIndexStatStore IndexStatStore { get; set; }
    }
}
