using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XiaoyaStore.Data;
using XiaoyaStore.Store;

namespace XiaoyaStoreUnitTest
{
    [TestClass]
    public class UrlFileUnitTest
    {
        [TestMethod]
        public async Task TestSaveOneUrl()
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

                using (var context = new XiaoyaSearchContext(options))
                {
                    var urlFileStore = new UrlFileStore(context);
                    await urlFileStore.SaveAsync(new XiaoyaStore.Data.Model.UrlFile
                    {
                        Url = "http://www.bnu.edu.cn",
                        FilePath = @"D:\a.html",
                        FileHash = "abcd",
                        Charset = "utf8",
                        MimeType = "text/html",
                    });
                }

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.UrlFiles.Count());
                    Assert.AreEqual("http://www.bnu.edu.cn", context.UrlFiles.Single().Url);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public async Task TestSaveTwoUrls()
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

                using (var context = new XiaoyaSearchContext(options))
                {
                    var urlFileStore = new UrlFileStore(context);
                    await urlFileStore.SaveAsync(new XiaoyaStore.Data.Model.UrlFile
                    {
                        Url = "http://www.bnu.edu.cn",
                        FilePath = @"D:\a.html",
                        FileHash = "abcd",
                        Charset = "utf8",
                        MimeType = "text/html",
                    });

                    await Task.Run(() => Thread.Sleep(3000));

                    await urlFileStore.SaveAsync(new XiaoyaStore.Data.Model.UrlFile
                    {
                        Url = "http://www.bnu.edu.cn/news",
                        FilePath = @"D:\b.html",
                        FileHash = "abcdef",
                        Charset = "utf8",
                        MimeType = "text/html",
                    });
                }

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(2, context.UrlFiles.Count());
                    var urlFiles = context.UrlFiles.OrderByDescending(o => o.UpdatedAt).ToList();
                    Assert.AreEqual("http://www.bnu.edu.cn/news", urlFiles[0].Url);
                    Assert.AreEqual("http://www.bnu.edu.cn", urlFiles[1].Url);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Test if UpdateInterval is updated correctly
        /// </summary>
        [TestMethod]
        public async Task TestSaveSameUrlTwice()
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

                using (var context = new XiaoyaSearchContext(options))
                {
                    var urlFileStore = new UrlFileStore(context);
                    await urlFileStore.SaveAsync(new XiaoyaStore.Data.Model.UrlFile
                    {
                        Url = "http://www.bnu.edu.cn",
                        FilePath = @"D:\a.html",
                        FileHash = "abcd",
                        Charset = "utf8",
                        MimeType = "text/html",
                    });
                }


                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.UrlFiles.Count());
                    var urlFile = context.UrlFiles.Single();
                    Assert.AreEqual(TimeSpan.FromDays(3), urlFile.UpdateInterval);
                }

                using (var context = new XiaoyaSearchContext(options))
                {
                    var urlFileStore = new UrlFileStore(context);
                    await urlFileStore.SaveAsync(new XiaoyaStore.Data.Model.UrlFile
                    {
                        Url = "http://www.bnu.edu.cn",
                        FilePath = @"D:\b.html",
                        FileHash = "abcdef",
                        Charset = "utf8",
                        MimeType = "text/html",
                    });
                }

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.UrlFiles.Count());
                    var urlFile = context.UrlFiles.Single();
                    Assert.AreNotEqual(TimeSpan.FromDays(3), urlFile.UpdateInterval);
                }
            }
            finally
            {
                connection.Close();
            }
        }


    }
}
