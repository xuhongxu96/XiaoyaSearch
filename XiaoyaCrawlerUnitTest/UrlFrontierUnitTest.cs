using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.UrlFrontier;
using XiaoyaLogger;
using XiaoyaStore.Data;
using XiaoyaStore.Store;

namespace XiaoyaCrawlerUnitTest
{
    [TestClass]
    public class UrlFrontierUnitTest
    {
        delegate void TestFunc(DbContextOptions<XiaoyaSearchContext> options, CrawlerConfig config);

        private string logDir = Path.Combine(Path.GetTempPath(), "Logs");
        private string fetchDir = Path.Combine(Path.GetTempPath(), "Fetched");

        private void InitDatabase(TestFunc func)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                .UseSqlite(connection)
                .Options;

            try
            {
                using (var context = new XiaoyaSearchContext(options))
                {
                    context.Database.EnsureCreated();
                }

                func(options, new CrawlerConfig
                {
                    InitUrls = new List<string>(),
                    UrlFileStore = new UrlFileStore(options),
                    UrlFrontierItemStore = new UrlFrontierItemStore(options),
                    LogDirectory = logDir,
                    FetchDirectory = fetchDir,
                });
            }
            finally
            {
                connection.Close();
            }

        }

        [TestMethod]
        public void TestConfigInitUrls()
        {
            InitDatabase((options, config) =>
            {
                config.InitUrls = new List<string> { "http://www.baidu.com" };
                var urlFrontier = new SimpleUrlFrontier(config);

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.UrlFrontierItems.Count());
                }
            });
        }

        [TestMethod]
        public void TestPushUrl()
        {
            InitDatabase((options, config) =>
            {
                var urlFrontier = new SimpleUrlFrontier(config);

                urlFrontier.PushUrls(new List<string> { "http://www.baidu.com" });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.UrlFrontierItems.Count());
                }

            });
        }

        [TestMethod]
        public void TestPopUrl()
        {
            InitDatabase((options, config) =>
            {
                var urlFrontier = new SimpleUrlFrontier(config);

                urlFrontier.PushUrls(new List<string> { "http://www.baidu.com" });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.UrlFrontierItems.Count());
                }

                var item = urlFrontier.PopUrl();
                Assert.AreEqual("http://www.baidu.com", item.Url);
            });
        }

        [TestMethod]
        public void TestPushBackUrl()
        {
            InitDatabase((options, config) =>
            {
                var urlFrontier = new SimpleUrlFrontier(config);

                urlFrontier.PushUrls(new List<string> { "http://www.baidu.com" });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.UrlFrontierItems.Count());
                }

                var item = urlFrontier.PopUrl();
                Assert.AreEqual("http://www.baidu.com", item.Url);

                urlFrontier.PushBackUrl("http://www.baidu.com");
                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.UrlFrontierItems.Count());
                    Assert.AreEqual(1, context.UrlFrontierItems.Single().FailedTimes);
                }
            });
        }


        [TestMethod]
        public void TestConcurrent()
        {
            const int concurrentN = 100;
            const int urlN = 100;

            bool isDeleted = false;
            while (!isDeleted)
            {
                try
                {
                    if (Directory.Exists(logDir))
                        Directory.Delete(logDir, true);
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

            var config = new CrawlerConfig
            {
                InitUrls = new List<string>(),
                UrlFileStore = new UrlFileStore(options),
                UrlFrontierItemStore = new UrlFrontierItemStore(options),
                LogDirectory = logDir,
                FetchDirectory = fetchDir,
            };

            using (var context = new XiaoyaSearchContext(options))
            {
                context.Database.EnsureCreated();
                context.RemoveRange(context.UrlFrontierItems);
                context.RemoveRange(context.UrlFiles);
                context.SaveChanges();
            }
            var tasks = new List<Task>();

            for (int i = 0; i < concurrentN; ++i)
            {
                var task = Task.Run(() =>
                {
                    var urlFrontier = new SimpleUrlFrontier(config);

                    for (int j = 0; j < urlN; ++j)
                    {
                        urlFrontier.PopUrl();
                        urlFrontier.PushUrls(new List<string> { j.ToString() });
                    }
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            using (var context = new XiaoyaSearchContext(options))
            {
                Assert.AreEqual(urlN, context.UrlFrontierItems.Count());
            }

            var mark = new int[urlN];

            lock (RuntimeLogger.ReadLock)
            {
                var logFile = Path.Combine(logDir, "Crawler.log");
                foreach (var line in File.ReadLines(logFile))
                {
                    if (line.StartsWith("Pushed Url: "))
                    {
                        var url = line.Substring("Pushed Url: ".Length);
                        mark[int.Parse(url)]++;
                    }
                }
            }

            Assert.IsTrue(mark.All(o => o == concurrentN));

        }
    }
}
