﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XiaoyaStore.Store;
using XiaoyaCrawler;
using XiaoyaCrawler.UrlFilter;
using XiaoyaCrawler.UrlFrontier;
using XiaoyaCrawler.Fetcher;
using XiaoyaCrawler.Parser;
using XiaoyaCrawler.SimilarContentManager;
using XiaoyaStore.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using XiaoyaCommon.ArgumentParser;

namespace XiaoyaCrawlerInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = Parser.ParseArguments<CrawlerArguments>(args);
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

            using (var context = new XiaoyaSearchContext(options))
            {
                context.Database.Migrate();
            }

            var config = new XiaoyaCrawler.Config.CrawlerConfig
            {
                InitUrls = arguments.InitUrl.Split(","),
                UrlFileStore = new UrlFileStore(options),
                UrlFrontierItemStore = new UrlFrontierItemStore(options),
                FetchDirectory = arguments.FetchDir,
                LogDirectory = arguments.LogDir,
                MaxFetchingConcurrency = int.Parse(arguments.ThreadCount),
            };

            var urlFilters = new List<IUrlFilter>
            {
                new DomainUrlFilter(@"bnu\.edu\.cn"),
                new UrlNormalizer(),
            };

            var crawler = new Crawler(
                config,
                new SimpleUrlFrontier(config),
                new SimpleFetcher(config),
                new SimpleParser(config),
                new SimpleSimilarContentManager(config),
                urlFilters
            );

            crawler.StartAsync().GetAwaiter().GetResult();
        }
    }
}
