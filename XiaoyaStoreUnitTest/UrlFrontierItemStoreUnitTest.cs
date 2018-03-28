using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Store;

namespace XiaoyaStoreUnitTest
{
    [TestClass]
    public class UrlFrontierItemStoreUnitTest
    {
        [TestMethod]
        public void TestInit()
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

                var urlFrontierItemStore = new UrlFrontierItemStore(options);
                urlFrontierItemStore.Init(new List<string>
                {
                    "http://baidu.com",
                    "http://xuhongxu.com",
                });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(2, context.UrlFrontierItems.Count());
                    Assert.AreEqual("http://baidu.com", context.UrlFrontierItems.ToList()[0].Url);
                    Assert.AreEqual("http://xuhongxu.com", context.UrlFrontierItems.ToList()[1].Url);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestSave()
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

                var urlFrontierItemStore = new UrlFrontierItemStore(options);
                urlFrontierItemStore.Save("http://baidu.com");

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.UrlFrontierItems.Count());
                    Assert.AreEqual("http://baidu.com", context.UrlFrontierItems.ToList()[0].Url);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestPushBack()
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

                var urlFrontierItemStore = new UrlFrontierItemStore(options);
                urlFrontierItemStore.Init(new List<string>
                {
                    "http://baidu.com",
                });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.UrlFrontierItems.Count());
                    Assert.AreEqual("http://baidu.com", context.UrlFrontierItems.Single().Url);
                }

                urlFrontierItemStore.PushBack("http://baidu.com");

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.UrlFrontierItems.Count());
                    var item = context.UrlFrontierItems.Single();
                    Assert.AreEqual("http://baidu.com", item.Url);
                    Assert.AreEqual(0, item.FailedTimes);
                }

                using (var context = new XiaoyaSearchContext(options))
                {
                    var item = context.UrlFrontierItems.First();
                    item.IsPopped = true;
                    context.SaveChanges();
                }

                urlFrontierItemStore.PushBack("http://baidu.com");

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.UrlFrontierItems.Count());
                    var item = context.UrlFrontierItems.Single();
                    Assert.AreEqual("http://baidu.com", item.Url);
                    Assert.AreEqual(1, item.FailedTimes);
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

                var urlFrontierItemStore = new UrlFrontierItemStore(options);
                urlFrontierItemStore.Init(new List<string>
                {
                    "http://baidu.com",
                    "http://xuhongxu.com",
                });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(2, context.UrlFrontierItems.Count());
                }

                var item = urlFrontierItemStore.LoadByUrl("http://xuhongxu.com");

                Assert.IsNotNull(item);
                Assert.AreEqual("http://xuhongxu.com", item.Url);
                Assert.IsTrue(item.PlannedTime <= DateTime.Now);
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

                var urlFrontierItemStore = new UrlFrontierItemStore(options);
                urlFrontierItemStore.Init(new List<string>
                {
                    "http://baidu.com",
                    "http://xuhongxu.com",
                });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(2, context.UrlFrontierItems.Count());
                }

                Assert.AreEqual(2, urlFrontierItemStore.Count());
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestPopUrlForCrawl()
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

                var urlFrontierItemStore = new UrlFrontierItemStore(options);
                urlFrontierItemStore.Init(new List<string>
                {
                    "http://baidu.com",
                    "http://xuhongxu.com",
                });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(2, context.UrlFrontierItems.Count());
                }

                var item = urlFrontierItemStore.PopUrlForCrawl();
                Assert.AreEqual("http://baidu.com", item.Url);
                Assert.IsTrue(item.IsPopped);

                item = urlFrontierItemStore.PopUrlForCrawl();
                Assert.AreEqual("http://xuhongxu.com", item.Url);
                Assert.IsTrue(item.IsPopped);

                item = urlFrontierItemStore.PopUrlForCrawl();
                Assert.IsNull(item);
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestPopUrlForCrawlAndPushBackWithoutUrlFile()
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

                var urlFrontierItemStore = new UrlFrontierItemStore(options);
                urlFrontierItemStore.Init(new List<string>
                {
                    "http://baidu.com",
                    "http://xuhongxu.com",
                });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(2, context.UrlFrontierItems.Count());
                }

                var item = urlFrontierItemStore.PopUrlForCrawl();
                Assert.AreEqual("http://baidu.com", item.Url);
                Assert.IsTrue(item.IsPopped);

                item = urlFrontierItemStore.PushBack("http://baidu.com");
                Assert.AreEqual("http://baidu.com", item.Url);
                Assert.AreEqual(1, item.FailedTimes);
                Assert.IsTrue(item.PlannedTime > DateTime.Now);
                Assert.IsFalse(item.IsPopped);

                item = urlFrontierItemStore.PopUrlForCrawl();
                Assert.AreEqual("http://xuhongxu.com", item.Url);
                Assert.IsTrue(item.IsPopped);

                item = urlFrontierItemStore.PopUrlForCrawl();
                Assert.IsNull(item);

                item = urlFrontierItemStore.PopUrlForCrawl(false);
                Assert.AreEqual("http://baidu.com", item.Url);
                Assert.IsTrue(item.IsPopped);
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestPopUrlForCrawlAndPushBackWithUrlFile()
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

                var urlFrontierItemStore = new UrlFrontierItemStore(options);
                urlFrontierItemStore.Init(new List<string>
                {
                    "http://baidu.com",
                    "http://xuhongxu.com",
                });

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(2, context.UrlFrontierItems.Count());
                }

                var item = urlFrontierItemStore.PopUrlForCrawl();
                Assert.AreEqual("http://baidu.com", item.Url);
                Assert.IsTrue(item.IsPopped);

                var urlFileStore = new UrlFileStore(options);
                urlFileStore.Save(new UrlFile
                {
                    Url = "http://baidu.com",
                    FilePath = @"D:\a.html",
                    FileHash = "abcd",
                    Charset = "utf8",
                    MimeType = "text/html",
                });

                item = urlFrontierItemStore.PushBack("http://baidu.com");
                Assert.AreEqual("http://baidu.com", item.Url);
                Assert.AreEqual(0, item.FailedTimes);
                Assert.IsTrue(item.PlannedTime > DateTime.Now);
                Assert.IsFalse(item.IsPopped);

                item = urlFrontierItemStore.PopUrlForCrawl();
                Assert.AreEqual("http://xuhongxu.com", item.Url);
                Assert.IsTrue(item.IsPopped);

                item = urlFrontierItemStore.PopUrlForCrawl();
                Assert.IsNull(item);

                item = urlFrontierItemStore.PopUrlForCrawl(false);
                Assert.AreEqual("http://baidu.com", item.Url);
                Assert.IsTrue(item.IsPopped);
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestPopUrlForCrawlInOrder()
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
                    context.UrlFrontierItems.AddRange(new List<UrlFrontierItem>
                    {
                        new UrlFrontierItem
                        {
                            Url = "a",
                            PlannedTime = DateTime.Now,
                            FailedTimes = 0,
                            IsPopped = false,
                            UpdatedAt = DateTime.Now,
                            CreatedAt = DateTime.Now,
                        },
                        new UrlFrontierItem
                        {
                            Url = "c",
                            PlannedTime = DateTime.Now.AddMinutes(2),
                            FailedTimes = 0,
                            IsPopped = false,
                            UpdatedAt = DateTime.Now,
                            CreatedAt = DateTime.Now,
                        },
                        new UrlFrontierItem
                        {
                            Url = "b",
                            PlannedTime = DateTime.Now.AddMinutes(1),
                            FailedTimes = 0,
                            IsPopped = false,
                            UpdatedAt = DateTime.Now,
                            CreatedAt = DateTime.Now,
                        },
                    });
                    context.SaveChanges();
                }

                var urlFrontierItemStore = new UrlFrontierItemStore(options);
                var item = urlFrontierItemStore.PopUrlForCrawl(false);
                Assert.AreEqual("a", item.Url);

                var urlFileStore = new UrlFileStore(options);
                urlFileStore.Save(new UrlFile
                {
                    Url = "a",
                    FilePath = @"D:\a.html",
                    FileHash = "abcd",
                    Charset = "utf8",
                    MimeType = "text/html",
                });

                urlFrontierItemStore.PushBack("a");

                item = urlFrontierItemStore.PopUrlForCrawl(false);
                Assert.AreEqual("b", item.Url);

                item = urlFrontierItemStore.PushBack("b");
                Assert.AreEqual(1, item.FailedTimes);

                item = urlFrontierItemStore.PopUrlForCrawl(false);
                Assert.AreEqual("c", item.Url);

                item = urlFrontierItemStore.PopUrlForCrawl(false);
                Assert.AreEqual("b", item.Url);

                item = urlFrontierItemStore.PopUrlForCrawl(false);
                Assert.AreEqual("a", item.Url);

                item = urlFrontierItemStore.PopUrlForCrawl(false);
                Assert.IsNull(item);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
