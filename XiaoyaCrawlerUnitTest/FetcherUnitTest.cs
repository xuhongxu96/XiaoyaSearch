using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.Fetcher;
using XiaoyaStore.Data;
using XiaoyaStore.Store;

namespace XiaoyaCrawlerUnitTest
{
    [TestClass]
    public class FetcherUnitTest
    {
        delegate void TestFunc(IFetcher fetcher);

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

                var config = new CrawlerConfig
                {
                    InitUrls = new List<string>(),
                    UrlFileStore = new UrlFileStore(options),
                    UrlFrontierItemStore = new UrlFrontierItemStore(options),
                    LogDirectory = logDir,
                    FetchDirectory = fetchDir,
                };
                var parser = new SimpleFetcher(config);
                func(parser);
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestFetchHttp()
        {
            InitDatabase(fetcher =>
            {
                var urlFile = fetcher.FetchAsync("http://www.bnu.edu.cn").GetAwaiter().GetResult();
                var filePath = urlFile.FilePath;
                Assert.IsTrue(File.Exists(filePath));
                var content = File.ReadAllText(filePath);
                Assert.IsTrue(content.Contains("北京师范大学"));
            });
        }

        [TestMethod]
        public void TestFetchHttps()
        {
            InitDatabase(fetcher =>
            {
                var urlFile = fetcher.FetchAsync("https://www.baidu.com").GetAwaiter().GetResult();
                var filePath = urlFile.FilePath;
                Assert.IsTrue(File.Exists(filePath));
                var content = File.ReadAllText(filePath);
                Assert.IsTrue(content.Contains("百度"));
            });
        }

        [TestMethod]
        public void TestNotSupportedProtocol()
        {
            InitDatabase(fetcher =>
            {
                Assert.ThrowsException<NotSupportedException>(() =>
                {
                    var urlFile = fetcher.FetchAsync("xxxx://bnu.edu.cn").GetAwaiter().GetResult();
                });
            });
        }

        [TestMethod]
        public void TestFetch404Page()
        {
            InitDatabase(fetcher =>
            {
                Assert.ThrowsException<IOException>(() =>
                {
                    var urlFile = fetcher.FetchAsync("http://www.bnu.edu.cn/xxxxx").GetAwaiter().GetResult();
                });
            });
        }
    }
}
