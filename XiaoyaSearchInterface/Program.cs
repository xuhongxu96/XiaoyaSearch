﻿using Microsoft.EntityFrameworkCore;
using System;
using XiaoyaCommon.ArgumentParser;
using XiaoyaSearch;
using XiaoyaSearch.Config;
using XiaoyaStore.Data;
using XiaoyaStore.Store;

namespace XiaoyaSearchInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = Parser.ParseArguments<SearchArguments>(args);
            DbContextOptions<XiaoyaSearchContext> options;

            switch (arguments.DbType)
            {
                case "sqlite":
                default:
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

            var config = new SearchEngineConfig
            {
                UrlFileStore = new UrlFileStore(options),
                InvertedIndexStore = new InvertedIndexStore(options),
                IndexStatStore = new IndexStatStore(options),
                UrlFileIndexStatStore = new UrlFileIndexStatStore(options),
            };

            var engine = new SearchEngine(config);
            var store = new UrlFileStore(options);

            while (true)
            {
                Console.WriteLine("Search:");
                var query = Console.ReadLine();

                var results = engine.Search(query);

                foreach (var result in results)
                {
                    var urlFile = store.LoadById(result.UrlFileId);

                    Console.WriteLine("{0}: {1} ({2})", result.UrlFileId, urlFile.Url, result.Score);
                }

                Console.WriteLine();
            }

        }
    }
}
