using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.Fetcher;
using XiaoyaCrawler.Parser;
using XiaoyaStore.Helper;
using XiaoyaStore.Store;

namespace XiaoyaCrawlerUnitTest
{
    [TestClass]
    public class ParserUnitTest
    {
        delegate void TestFunc(IParser parser);

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
        public void TestParse()
        {
            var parser = new SimpleParser(InitConfig());
            {
                var tempHtml = Path.GetTempFileName();

                File.WriteAllText(tempHtml, @"
<!doctype>
<html lang=""zh_CN"">
<head>
<title>Hello, World! 你好，世界！</title>
</head>
<body>
<h1>Hello, World! 你好，世界！</h1>
<a href=""http://www.baidu.com"">百度</a>
<a href=""https://www.google.com"">Google</a>
<a href=""../index.htm"">返回</a>
</body>
</html>
", Encoding.UTF8);
                var file = new FetchedFile
                {
                    Url = "http://bar.foo.com/a/b/c/",
                    FilePath = tempHtml,
                    Charset = "UTF-8",
                    MimeType = "text/html",
                    FileHash = HashHelper.GetFileMd5(tempHtml),
                };
                var parseResult = parser.ParseAsync(file).GetAwaiter().GetResult();

                Assert.AreEqual(
                    Regex.Replace("hello, world! 你好,世界!百度google返回", @"\s", ""),
                    Regex.Replace(parseResult.TextContent, @"\s", "")
                    );
                var urls = parseResult.Links.Select(o => o.Url).ToList();
                Assert.AreEqual(3, urls.Count);
                Assert.AreEqual("http://www.baidu.com/", urls[0]);
                Assert.AreEqual("https://www.google.com/", urls[1]);
                Assert.AreEqual("http://bar.foo.com/a/b/index.htm", urls[2]);
            }
        }

        [TestMethod]
        public void TestNotSupportedType()
        {
            var parser = new SimpleParser(InitConfig());
            {
                var tempXXX = Path.GetTempFileName();

                File.WriteAllText(tempXXX, "xxx", Encoding.UTF8);
                var urlFile = new FetchedFile
                {
                    Url = "http://bar.foo.com/a/b/c/",
                    FilePath = tempXXX,
                    Charset = "UTF-8",
                    MimeType = "xxx/xxx",
                    FileHash = HashHelper.GetFileMd5(tempXXX),
                };

                Assert.ThrowsException<NotSupportedException>(() =>
                {
                    parser.ParseAsync(urlFile).GetAwaiter().GetResult();
                });
            }
        }
    }
}
