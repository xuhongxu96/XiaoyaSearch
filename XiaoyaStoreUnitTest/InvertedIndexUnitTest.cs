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
    public class InvertedIndexUnitTest
    {

        private async Task<UrlFile> InitDatabase(XiaoyaSearchContext context)
        {
            var urlFileStore = new UrlFileStore(context);
            return await urlFileStore.SaveAsync(new XiaoyaStore.Data.Model.UrlFile
            {
                Url = "http://www.bnu.edu.cn",
                FilePath = @"D:\a.html",
                FileHash = "abcd",
                Charset = "utf8",
                MimeType = "text/html",
            });
        }

        [TestMethod]
        public async Task TestSave()
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
                    var urlFile = await InitDatabase(context);

                    var invertedIndexStore = new InvertedIndexStore(context);
                    var invertedIndex = new InvertedIndex
                    {
                        Word = "你好",
                        Position = 0,
                        UrlFileId = urlFile.UrlFileId,
                    };
                    await invertedIndexStore.SaveInvertedIndexAsync(invertedIndex);
                }

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
        public async Task TestSaveMany()
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
                    var urlFile = await InitDatabase(context);

                    var invertedIndexStore = new InvertedIndexStore(context);
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
                    await invertedIndexStore.SaveInvertedIndicesAsync(invertedIndices);
                }

                using (var context = new XiaoyaSearchContext(options))
                {
                    Assert.AreEqual(3, context.InvertedIndices.Count());
                    var invertedIndices = context.InvertedIndices.OrderBy(o => o.Position).ToList();
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
        public async Task TestClear()
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
                    var urlFile = await InitDatabase(context);

                    var invertedIndexStore = new InvertedIndexStore(context);
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
                    await invertedIndexStore.SaveInvertedIndicesAsync(invertedIndices);

                    await invertedIndexStore.ClearInvertedIndicesOf(urlFile);

                    var invertedIndex = new InvertedIndex
                    {
                        Word = "你好",
                        Position = 0,
                        UrlFileId = urlFile.UrlFileId,
                    };
                    await invertedIndexStore.SaveInvertedIndexAsync(invertedIndex);
                }

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
        public async Task TestLoadByWord()
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
                    var urlFile = await InitDatabase(context);

                    var invertedIndexStore = new InvertedIndexStore(context);
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
                        new InvertedIndex
                        {
                            Word = "你好",
                            Position = 5,
                            UrlFileId = urlFile.UrlFileId,
                        },
                    };
                    await invertedIndexStore.SaveInvertedIndicesAsync(invertedIndices);
                }

                using (var context = new XiaoyaSearchContext(options))
                {
                    var invertedIndexStore = new InvertedIndexStore(context);

                    var invertedIndices = invertedIndexStore.LoadByWord("你好")
                        .OrderBy(o => o.Position)
                        .ToList();
                    Assert.AreEqual(2, invertedIndices.Count);
                    Assert.AreEqual(0, invertedIndices[0].Position);
                    Assert.AreEqual(5, invertedIndices[1].Position);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public async Task TestLoadByUrlFilePosition()
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

                using (var context = new XiaoyaSearchContext(options))
                {
                    urlFile = await InitDatabase(context);

                    var invertedIndexStore = new InvertedIndexStore(context);
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
                        new InvertedIndex
                        {
                            Word = "你好",
                            Position = 5,
                            UrlFileId = urlFile.UrlFileId,
                        },
                    };
                    await invertedIndexStore.SaveInvertedIndicesAsync(invertedIndices);
                }

                using (var context = new XiaoyaSearchContext(options))
                {
                    var invertedIndexStore = new InvertedIndexStore(context);

                    var invertedIndex = invertedIndexStore.LoadByUrlFilePosition(urlFile, 4);
                    Assert.AreEqual("是", invertedIndex.Word);

                    invertedIndex = invertedIndexStore.LoadByUrlFilePosition(urlFile, 3);
                    Assert.AreEqual("我们", invertedIndex.Word);
                }
            }
            finally
            {
                connection.Close();
            }
        }
        
    }
}
