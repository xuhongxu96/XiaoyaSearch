using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XiaoyaCrawler;
using XiaoyaCrawler.Fetcher;
using XiaoyaCrawler.Parser;
using XiaoyaCrawler.SimilarContentManager;
using XiaoyaCrawler.UrlFilter;
using XiaoyaCrawler.UrlFrontier;
using XiaoyaLogger;
using XiaoyaStore.Store;

namespace XiaoyaCrawlerUnitTest
{
    [TestClass]
    public class CrawlerUnitTest
    {
        private string logDir = Path.Combine(Path.GetTempPath(), "Logs");
        private string fetchDir = "Fetched";

        [TestMethod]
        public async Task TestCrawler()
        {
            var config = new XiaoyaCrawler.Config.CrawlerConfig
            {
                InitUrls = new List<string>
                {
                    "http://www.bnu.edu.cn",
                },
                UrlFileStore = new UrlFileStore(),
                UrlFrontierItemStore = new UrlFrontierItemStore(),
                LinkStore = new LinkStore(),
                FetchDirectory = fetchDir,
                LogDirectory = logDir,
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

            var task = Task.Run(() =>
            {
                crawler.StartAsync().GetAwaiter().GetResult();
            });

            Thread.Sleep(10000);

            await crawler.StopAsync();

            lock (RuntimeLogger.ReadLock)
            {
                var urlLineNo = new Dictionary<string, int>();
                int lineNo = 0;
                foreach (var line in File.ReadLines(Path.Combine(logDir, "Crawler.log")))
                {
                    lineNo++;
                    if (!line.Contains(":") || line.Length <= line.IndexOf(":") + 2) continue;
                    var url = line.Substring(line.IndexOf(":") + 2);
                    if (line.StartsWith("Begin Crawl: "))
                    {
                        if (urlLineNo.ContainsKey(url) && urlLineNo[url] != -1)
                        {
                            Assert.Fail("Duplicate Crawl: " + urlLineNo[url] + ":" + lineNo + " " + url);
                        }
                        else
                        {
                            urlLineNo[url] = lineNo;
                        }
                    }
                    else if (line.StartsWith("End Crawl: "))
                    {
                        urlLineNo[url] = -1;
                    }
                }
            }
        }
    }
}
