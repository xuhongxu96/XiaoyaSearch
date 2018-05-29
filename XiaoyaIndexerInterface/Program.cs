using System;
using System.IO;
using XiaoyaCommon.ArgumentParser;
using XiaoyaIndexer;
using XiaoyaIndexer.Config;
using XiaoyaLogger;
using XiaoyaStore.Store;

namespace XiaoyaIndexerInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = Parser.ParseArguments<IndexerArguments>(args);

            var config = new IndexerConfig
            {
                LogDirectory = arguments.LogDir,
                UrlFileStore = new UrlFileStore(),
                LinkStore = new LinkStore(),
                InvertedIndexStore = new InvertedIndexStore(),
                PostingListStore = new PostingListStore(),
                MaxIndexingConcurrency = int.Parse(arguments.ThreadCount),
            };

            var indexer = new SimpleIndexer(config);
            indexer.CreateIndexAsync().GetAwaiter().GetResult();

        }
    }
}
