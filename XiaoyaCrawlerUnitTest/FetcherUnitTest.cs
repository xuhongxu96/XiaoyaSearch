using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.Fetcher;
using XiaoyaStore.Store;

namespace XiaoyaCrawlerUnitTest
{
    [TestClass]
    public class FetcherUnitTest
    {
        delegate void TestFunc(IFetcher fetcher);

        private string logDir = Path.Combine(Path.GetTempPath(), "Logs");
        private string fetchDir = Path.Combine(Path.GetTempPath(), "Fetched");

        private CrawlerConfig InitConfig()
        {
            return new CrawlerConfig
            {
                InitUrls = new List<string>(),
                UrlFileStore = new UrlFileStore(),
                UrlFrontierItemStore = new UrlFrontierItemStore(),
                LogDirectory = logDir,
                FetchDirectory = fetchDir,
            };
        }

        [TestMethod]
        public void TestFetchHttp()
        {
            var fetcher = new SimpleFetcher(InitConfig());
            {
                var file = fetcher.FetchAsync("http://www.bnu.edu.cn").GetAwaiter().GetResult();
                var filePath = file.FilePath;
                Assert.IsTrue(File.Exists(filePath));
                var content = File.ReadAllText(filePath);
                Assert.IsTrue(content.Contains("北京师范大学"));
            }
        }

        [TestMethod]
        public void TestFetchHttps()
        {
            var fetcher = new SimpleFetcher(InitConfig());
            {
                var file = fetcher.FetchAsync("https://www.baidu.com").GetAwaiter().GetResult();
                var filePath = file.FilePath;
                Assert.IsTrue(File.Exists(filePath));
                var content = File.ReadAllText(filePath);
                Assert.IsTrue(content.Contains("百度"));
            }
        }

        [TestMethod]
        public void TestNotSupportedProtocol()
        {
            var fetcher = new SimpleFetcher(InitConfig());
            {
                Assert.ThrowsException<NotSupportedException>(() =>
                {
                    var file = fetcher.FetchAsync("xxxx://bnu.edu.cn").GetAwaiter().GetResult();
                });
            }
        }

        [TestMethod]
        public void TestFetch404Page()
        {
            var fetcher = new SimpleFetcher(InitConfig());
            {
                Assert.ThrowsException<IOException>(() =>
                {
                    var file = fetcher.FetchAsync("http://www.bnu.edu.cn/xxxxx").GetAwaiter().GetResult();
                });
            }
        }
    }
}
