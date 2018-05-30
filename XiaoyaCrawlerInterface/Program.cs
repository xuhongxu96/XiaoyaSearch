using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XiaoyaStore.Store;
using XiaoyaCrawler;
using XiaoyaCrawler.UrlFilter;
using XiaoyaCrawler.UrlFrontier;
using XiaoyaCrawler.Fetcher;
using XiaoyaCrawler.Parser;
using XiaoyaCrawler.SimilarContentManager;
using System.IO;
using XiaoyaCommon.ArgumentParser;
using XiaoyaLogger;

namespace XiaoyaCrawlerInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = Parser.ParseArguments<CrawlerArguments>(args);

            var config = new XiaoyaCrawler.Config.CrawlerConfig
            {
                InitUrls = arguments.InitUrl.Split(","),
                UrlFileStore = new UrlFileStore(),
                UrlFrontierItemStore = new UrlFrontierItemStore(),
                LinkStore = new LinkStore(),
                PostingListStore = new PostingListStore(),
                InvertedIndexStore = new InvertedIndexStore(),
                FetchDirectory = arguments.FetchDir,
                LogDirectory = arguments.LogDir,
                MaxFetchingConcurrency = int.Parse(arguments.ThreadCount),
            };

            var urlFilters = new List<IUrlFilter>
            {
                new DomainUrlFilter(@"^http(s)?://[\w\.\-_]*(bnu\.edu\.cn|oiegg.com($|/$|/(index|viewthread|forumdisplay).php))",
                    @"(v6\.oiegg\.com)"
                    + @"|http://[\w\.\-_]*(oiegg.com)"
                    + @"|((cless|pb\.ss\.graduate|ipv6te)\.bnu\.edu\.cn)"
                    + @"|brain\.bnu\.edu\.cn/mrbs"
                    + @"|cogs\.bnu\.edu\.cn/index\.php\?action=page&pid=1[123]"
                    + @"|532movie\.bnu\.edu\.cn/player"
                    + @"|(/(search|print|login|space)[\./])"),
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
