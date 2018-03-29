using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XiaoyaStore.Data;
using XiaoyaStore.Store;
using static XiaoyaStore.Data.Model.UrlFile;

namespace XiaoyaStoreUnitTest
{
    [TestClass]
    public class UrlFileStoreUnitTest
    {
        [TestMethod]
        public void TestSaveOneUrl()
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

                var urlFileStore = new UrlFileStore(options);
                urlFileStore.Save(new XiaoyaStore.Data.Model.UrlFile
                {
                    Url = "http://www.bnu.edu.cn",
                    FilePath = @"D:\a.html",
                    Content = "abcd",
                    Charset = "utf8",
                    MimeType = "text/html",
                });

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
        public void TestSaveTwoUrls()
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

                var urlFileStore = new UrlFileStore(options);
                urlFileStore.Save(new XiaoyaStore.Data.Model.UrlFile
                {
                    Url = "http://www.bnu.edu.cn",
                    FilePath = @"D:\a.html",
                    Content = "abcd",
                    Charset = "utf8",
                    MimeType = "text/html",
                });

                Thread.Sleep(1000);

                urlFileStore.Save(new XiaoyaStore.Data.Model.UrlFile
                {
                    Url = "http://www.bnu.edu.cn/news",
                    FilePath = @"D:\b.html",
                    Content = "abcdef",
                    Charset = "utf8",
                    MimeType = "text/html",
                });

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

        [TestMethod]
        public void TestCount()
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

                var urlFileStore = new UrlFileStore(options);
                urlFileStore.Save(new XiaoyaStore.Data.Model.UrlFile
                {
                    Url = "http://www.bnu.edu.cn",
                    FilePath = @"D:\a.html",
                    Content = "abcd",
                    Charset = "utf8",
                    MimeType = "text/html",
                });

                Thread.Sleep(1000);

                urlFileStore.Save(new XiaoyaStore.Data.Model.UrlFile
                {
                    Url = "http://www.bnu.edu.cn/news",
                    FilePath = @"D:\b.html",
                    Content = "abcdef",
                    Charset = "utf8",
                    MimeType = "text/html",
                });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(2, urlFileStore.Count());
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
        public void TestSaveSameUrlTwice()
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

                var urlFileStore = new UrlFileStore(options);
                urlFileStore.Save(new XiaoyaStore.Data.Model.UrlFile
                {
                    Url = "http://www.bnu.edu.cn",
                    FilePath = @"D:\a.html",
                    Content = "abcd",
                    Charset = "utf8",
                    MimeType = "text/html",
                });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.UrlFiles.Count());
                    var urlFile = context.UrlFiles.Single();
                    Assert.AreEqual(TimeSpan.FromDays(3), urlFile.UpdateInterval);
                }

                urlFileStore.Save(new XiaoyaStore.Data.Model.UrlFile
                {
                    Url = "http://www.bnu.edu.cn",
                    FilePath = @"D:\b.html",
                    Content = "abcdef",
                    Charset = "utf8",
                    MimeType = "text/html",
                });

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

        [TestMethod]
        public void TestLoadByUrl()
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

                var urlFileStore = new UrlFileStore(options);
                urlFileStore.Save(new XiaoyaStore.Data.Model.UrlFile
                {
                    Url = "http://www.bnu.edu.cn",
                    FilePath = @"D:\a.html",
                    Content = "abcd",
                    Charset = "utf8",
                    MimeType = "text/html",
                });
                urlFileStore.Save(new XiaoyaStore.Data.Model.UrlFile
                {
                    Url = "http://www.bnu.edu.cn/news",
                    FilePath = @"D:\b.html",
                    Content = "abcdef",
                    Charset = "utf8",
                    MimeType = "text/html",
                });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(2, context.UrlFiles.Count());
                }

                Assert.AreEqual(@"D:\a.html", urlFileStore.LoadByUrl("http://www.bnu.edu.cn").FilePath);
                Assert.AreEqual(@"D:\b.html", urlFileStore.LoadByUrl("http://www.bnu.edu.cn/news").FilePath);
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestLoadByFilePath()
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

                var urlFileStore = new UrlFileStore(options);
                urlFileStore.Save(new XiaoyaStore.Data.Model.UrlFile
                {
                    Url = "http://www.bnu.edu.cn",
                    FilePath = @"D:\a.html",
                    Content = "abcd",
                    Charset = "utf8",
                    MimeType = "text/html",
                });
                urlFileStore.Save(new XiaoyaStore.Data.Model.UrlFile
                {
                    Url = "http://www.bnu.edu.cn/news",
                    FilePath = @"D:\b.html",
                    Content = "abcdef",
                    Charset = "utf8",
                    MimeType = "text/html",
                });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(2, context.UrlFiles.Count());
                }

                Assert.AreEqual(@"http://www.bnu.edu.cn", urlFileStore.LoadByFilePath(@"D:\a.html").Url);
                Assert.AreEqual(@"http://www.bnu.edu.cn/news", urlFileStore.LoadByFilePath(@"D:\b.html").Url);
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestLoadForIndex()
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

                var urlFileStore = new UrlFileStore(options);
                urlFileStore.Save(new XiaoyaStore.Data.Model.UrlFile
                {
                    Url = "http://www.bnu.edu.cn",
                    FilePath = @"D:\a.html",
                    Content = "abcd",
                    Charset = "utf8",
                    MimeType = "text/html",
                });
                urlFileStore.Save(new XiaoyaStore.Data.Model.UrlFile
                {
                    Url = "http://www.bnu.edu.cn/news",
                    FilePath = @"D:\b.html",
                    Content = "abcdef",
                    Charset = "utf8",
                    MimeType = "text/html",
                });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(2, context.UrlFiles.Count());
                }

                var urlFile = urlFileStore.LoadAnyForIndex();
                var urlFile2 = urlFileStore.LoadAnyForIndex();

                Assert.AreNotEqual(urlFile.Url, urlFile2.Url);

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.IsTrue(context.UrlFiles.All(o => o.IndexStatus == UrlFileIndexStatus.Indexing));
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
