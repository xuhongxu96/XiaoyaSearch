using Microsoft.EntityFrameworkCore;
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
using XiaoyaStore.Data;
using XiaoyaStore.Store;

namespace XiaoyaCrawlerUnitTest
{
    [TestClass]
    public class CrawlerUnitTest
    {
        private string logDir = Path.Combine(Path.GetTempPath(), "Logs");
        private string fetchDir = Path.Combine(Path.GetTempPath(), "Fetched");

        [TestMethod]
        public async Task TestCrawler()
        {
            bool isDeleted = false;
            while (!isDeleted)
            {
                try
                {
                    if (Directory.Exists(logDir))
                        Directory.Delete(logDir, true);
                    if (Directory.Exists(fetchDir))
                        Directory.Delete(fetchDir, true);
                    isDeleted = true;
                }
                catch (IOException)
                {
                    Thread.Sleep(500);
                }
            }

            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                .UseSqlite("Data Source=XiaoyaSearch.db")
                .Options;

            using (var context = new XiaoyaSearchContext(options))
            {
                context.Database.EnsureCreated();
                context.RemoveRange(context.UrlFiles);
                context.RemoveRange(context.UrlFrontierItems);
                context.SaveChanges();
            }

            var config = new XiaoyaCrawler.Config.CrawlerConfig
            {
                InitUrls = new List<string>
                {
                    "http://www.bnu.edu.cn",
                },
                UrlFileStore = new UrlFileStore(options),
                UrlFrontierItemStore = new UrlFrontierItemStore(options),
                FetchDirectory = fetchDir,
                LogDirectory = logDir,
                MaxFetchingConcurrency = 100,
            };

            var urlFilters = new List<IUrlFilter>
            {
                new DomainUrlFilter(@"^http\://(www\.)?bnu\.edu\.cn(/[a-zA-Z0-9/]*)?$"),
                //new DomainUrlFilter(@"^http\://(www\.)?bnu\.edu\.cn/?$"),
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

            Thread.Sleep(20000);

            await crawler.StopAsync();

            var urlFileCount = 0;

            using (var context = new XiaoyaSearchContext(options))
            {
                urlFileCount = context.UrlFiles.Count();
                Assert.IsTrue(urlFileCount > 0);
                Assert.IsTrue(context.UrlFrontierItems.Count() > 0);
            }

            Assert.AreEqual(urlFileCount, Directory.GetFiles(fetchDir).Length);

            lock (RuntimeLogger.ReadLock)
            {
                var urlLineNo = new Dictionary<string, int>();
                int lineNo = 0;
                foreach (var line in File.ReadLines(Path.Combine(logDir, "Crawler.log")))
                {
                    lineNo++;
                    if (!line.Contains(":")) continue;
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
