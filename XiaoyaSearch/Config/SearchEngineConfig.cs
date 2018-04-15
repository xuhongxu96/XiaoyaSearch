﻿using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaNLP.TextSegmentation;
using XiaoyaQueryParser.Config;
using XiaoyaRanker.Config;
using XiaoyaRetriever.Config;
using XiaoyaStore.Store;

namespace XiaoyaSearch.Config
{
    public class SearchEngineConfig
    {
        public ITextSegmenter TextSegmenter { get; set; }
            = new MaxMatchSegmenter();

        public IUrlFileStore UrlFileStore { get; set; }
        public IUrlFileIndexStatStore UrlFileIndexStatStore { get; set; }
        public IIndexStatStore IndexStatStore { get; set; }
        public IInvertedIndexStore InvertedIndexStore { get; set; }
    }
}