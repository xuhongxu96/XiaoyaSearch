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
    public class InvertedIndexStoreUnitTest
    {

        private UrlFile InitDatabase(DbContextOptions<XiaoyaSearchContext> options)
        {
            using (var context = new XiaoyaSearchContext(options))
            {
                var urlFile = new UrlFile
                {
                    Url = "http://www.bnu.edu.cn",
                    FilePath = @"D:\a.html",
                    FileHash = "abcd",
                    Charset = "utf8",
                    MimeType = "text/html",
                };
                context.UrlFiles.Add(urlFile);
                context.SaveChanges();
                return urlFile;
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

                var urlFile = InitDatabase(options);

                var invertedIndexStore = new InvertedIndexStore(options);
                var invertedIndex = new InvertedIndex
                {
                    Word = "你好",
                    Position = 0,
                    UrlFileId = urlFile.UrlFileId,
                };
                invertedIndexStore.SaveInvertedIndex(invertedIndex);

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(1, context.InvertedIndices.Count());
                    Assert.AreEqual("你好", context.InvertedIndices.Single().Word);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestSaveMany()
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

                var urlFile = InitDatabase(options);

                var invertedIndexStore = new InvertedIndexStore(options);
                var invertedIndices = new List<InvertedIndex>
                    {
                        new InvertedIndex
                        {
                            Word = "你好",
                            Position = 0,
                            UrlFileId = urlFile.UrlFileId,
                        },
                        new InvertedIndex
                        {
                            Word = "我们",
                            Position = 2,
                            UrlFileId = urlFile.UrlFileId,
                        },
                        new InvertedIndex
                        {
                            Word = "是",
                            Position = 4,
                            UrlFileId = urlFile.UrlFileId,
                        },
                    };
                invertedIndexStore.SaveInvertedIndices(invertedIndices);

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(3, context.InvertedIndices.Count());
                    invertedIndices = context.InvertedIndices.OrderBy(o => o.Position).ToList();
                    Assert.AreEqual("你好", invertedIndices[0].Word);
                    Assert.AreEqual("我们", invertedIndices[1].Word);
                    Assert.AreEqual("是", invertedIndices[2].Word);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestClear()
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

                var urlFile = InitDatabase(options);

                using (var context = new XiaoyaSearchContext(options))
                {
                    var invertedIndices = new List<InvertedIndex>
                    {
                        new InvertedIndex
                        {
                            Word = "你好",
                            Position = 0,
                            UrlFileId = urlFile.UrlFileId,
                        },
                        new InvertedIndex
                        {
                            Word = "我们",
                            Position = 2,
                            UrlFileId = urlFile.UrlFileId,
                        },
                        new InvertedIndex
                        {
                            Word = "是",
                            Position = 4,
                            UrlFileId = urlFile.UrlFileId,
                        },
                    };
                    context.InvertedIndices.AddRange(invertedIndices);
                    context.SaveChanges();
                }

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(3, context.InvertedIndices.Count());
                }

                var invertedIndexStore = new InvertedIndexStore(options);
                invertedIndexStore.ClearInvertedIndicesOf(urlFile);

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(0, context.InvertedIndices.Count());
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestLoadByWord()
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

                var urlFile = InitDatabase(options);

                using (var context = new XiaoyaSearchContext(options))
                {
                    var indices = new List<InvertedIndex>
                    {
                        new InvertedIndex
                        {
                            Word = "你好",
                            Position = 0,
                            UrlFileId = urlFile.UrlFileId,
                        },
                        new InvertedIndex
                        {
                            Word = "我们",
                            Position = 2,
                            UrlFileId = urlFile.UrlFileId,
                        },
                        new InvertedIndex
                        {
                            Word = "是",
                            Position = 4,
                            UrlFileId = urlFile.UrlFileId,
                        },
                        new InvertedIndex
                        {
                            Word = "你好",
                            Position = 5,
                            UrlFileId = urlFile.UrlFileId,
                        },
                    };
                    context.InvertedIndices.AddRange(indices);
                    context.SaveChanges();
                }

                var invertedIndexStore = new InvertedIndexStore(options);
                var invertedIndices = invertedIndexStore.LoadByWord("你好")
                    .OrderBy(o => o.Position)
                    .ToList();

                Assert.AreEqual(2, invertedIndices.Count);
                Assert.AreEqual(0, invertedIndices[0].Position);
                Assert.AreEqual(5, invertedIndices[1].Position);
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public void TestLoadByUrlFilePosition()
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

                UrlFile urlFile;

                urlFile = InitDatabase(options);

                using (var context = new XiaoyaSearchContext(options))
                {
                    var indices = new List<InvertedIndex>
                    {
                        new InvertedIndex
                        {
                            Word = "你好",
                            Position = 0,
                            UrlFileId = urlFile.UrlFileId,
                        },
                        new InvertedIndex
                        {
                            Word = "我们",
                            Position = 2,
                            UrlFileId = urlFile.UrlFileId,
                        },
                        new InvertedIndex
                        {
                            Word = "是",
                            Position = 4,
                            UrlFileId = urlFile.UrlFileId,
                        },
                        new InvertedIndex
                        {
                            Word = "你好",
                            Position = 5,
                            UrlFileId = urlFile.UrlFileId,
                        },
                    };
                    context.InvertedIndices.AddRange(indices);
                    context.SaveChanges();
                }

                var invertedIndexStore = new InvertedIndexStore(options);

                var invertedIndex = invertedIndexStore.LoadByUrlFilePosition(urlFile, 4);
                Assert.AreEqual("是", invertedIndex.Word);

                invertedIndex = invertedIndexStore.LoadByUrlFilePosition(urlFile, 3);
                Assert.AreEqual("我们", invertedIndex.Word);
            }
            finally
            {
                connection.Close();
            }
        }

    }
}
