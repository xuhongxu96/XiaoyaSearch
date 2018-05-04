using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XiaoyaSearch;
using XiaoyaStore.Store;

namespace XiaoyaSearchWeb
{
    public static class EngineOptions
    {
        public static UrlFileStore UrlFileStore { get; set; }
        public static InvertedIndexStore InvertedIndexStore { get; set; }
        public static IndexStatStore IndexStatStore { get; set; }

        public static SearchEngine SearchEngine { get; set; }
    }
}
