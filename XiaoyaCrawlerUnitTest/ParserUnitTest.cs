using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.Parser;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;
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
        private string checkPointDirectory = Path.Combine(Path.GetTempPath(), "CheckPoint");

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
                var parser = new SimpleParser(config);
                func(parser);
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestParse()
        {
            InitDatabase(parser =>
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
                var urlFile = new UrlFile
                {
                    Url = "http://bar.foo.com/a/b/c/",
                    FilePath = tempHtml,
                    Charset = "UTF-8",
                    MimeType = "text/html",
                    FileHash = HashHelper.GetFileMd5(tempHtml),
                };
                var parseResult = parser.ParseAsync(urlFile).GetAwaiter().GetResult();

                Assert.AreEqual(
                    Regex.Replace("hello, world! 你好,世界!百度google返回", @"\s", ""),
                    Regex.Replace(parseResult.TextContent, @"\s", "")
                    );
                var urls = parseResult.Links.Select(o => o.Url).ToList();
                Assert.AreEqual(3, urls.Count);
                Assert.AreEqual("http://www.baidu.com/", urls[0]);
                Assert.AreEqual("https://www.google.com/", urls[1]);
                Assert.AreEqual("http://bar.foo.com/a/b/index.htm", urls[2]);
            });
        }

        [TestMethod]
        public void TestNotSupportedType()
        {
            InitDatabase(parser =>
            {
                var tempXXX = Path.GetTempFileName();

                File.WriteAllText(tempXXX, "xxx", Encoding.UTF8);
                var urlFile = new UrlFile
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
            });
        }
    }
}
