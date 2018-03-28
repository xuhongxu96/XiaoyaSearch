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

            if (!Directory.Exists(arguments.DbDir))
            {
                Directory.CreateDirectory(arguments.DbDir);
            }

            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                .UseSqlite("Data Source=" + Path.Combine(arguments.DbDir, "XiaoyaSearch.db"))
                .Options;

            using (var context = new XiaoyaSearchContext(options))
            {
                context.Database.EnsureCreated();
            }

            var config = new XiaoyaCrawler.Config.CrawlerConfig
            {
                InitUrls = arguments.InitUrl.Split(","),
                UrlFileStore = new UrlFileStore(options),
                UrlFrontierItemStore = new UrlFrontierItemStore(options),
                FetchDirectory = arguments.FetchDir,
                LogDirectory = arguments.LogDir,
                MaxFetchingConcurrency = 100,
            };

            var urlFilters = new List<IUrlFilter>
            {
                new DomainUrlFilter(@"bnu\.edu\.cn"),
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
