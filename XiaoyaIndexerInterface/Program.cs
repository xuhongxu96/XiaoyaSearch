using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using XiaoyaCommon.ArgumentParser;
using XiaoyaIndexer;
using XiaoyaIndexer.Config;
using XiaoyaStore.Data;
using XiaoyaStore.Store;

namespace XiaoyaIndexerInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = Parser.ParseArguments<IndexerArguments>(args);
            DbContextOptions<XiaoyaSearchContext> options;

            switch (arguments.DbType)
            {
                case "sqlite":
                default:
                    if (!Directory.Exists(arguments.DbConnectionString))
                    {
                        Directory.CreateDirectory(arguments.DbConnectionString);
                    }
                    options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                                .UseSqlite(arguments.DbConnectionString)
                                .Options;
                    break;
                case "sqlserver":
                    options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                                .UseSqlServer(arguments.DbConnectionString)
                                .Options;
                    break;
            }

            var config = new IndexerConfig
            {
                LogDirectory = arguments.LogDir,
                UrlFileStore = new UrlFileStore(options),
                InvertedIndexStore = new InvertedIndexStore(options),
            };

            var indexer = new SimpleIndexer(config);
            indexer.CreateIndexAsync().GetAwaiter().GetResult();

        }
    }
}
