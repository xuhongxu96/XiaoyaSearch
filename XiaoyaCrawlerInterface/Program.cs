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

namespace XiaoyaCrawlerInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new XiaoyaCrawler.Config.CrawlerConfig
            {
                InitUrls = new List<string>
                {
                    "http://www.bnu.edu.cn",
                },
                UrlFileStore = new UrlFileStore(),
                UrlFrontierItemStore = new UrlFrontierItemStore(),
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