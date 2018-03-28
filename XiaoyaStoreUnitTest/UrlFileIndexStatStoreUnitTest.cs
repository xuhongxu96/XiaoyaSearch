using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Data;
using XiaoyaStore.Store;

namespace XiaoyaStoreUnitTest
{
    [TestClass]
    public class UrlFileIndexStatStoreUnitTest
    {
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
                    context.Add(new XiaoyaStore.Data.Model.UrlFileIndexStat
                    {
                        UrlFileId = 1,
                        Word = "你好",
                        WordFrequency = 100,
                    });
                    context.Add(new XiaoyaStore.Data.Model.UrlFileIndexStat
                    {
                        UrlFileId = 1,
                        Word = "我们",
                        WordFrequency = 200,
                    });
                    context.Add(new XiaoyaStore.Data.Model.UrlFileIndexStat
                    {
                        UrlFileId = 2,
                        Word = "我们",
                        WordFrequency = 300,
                    });
                    context.SaveChanges();
                }

                using (var context = new XiaoyaSearchContext(options))
                {
                    var store = new UrlFileIndexStatStore(options);
                    var word = store.LoadByWord("你好").ToList();
                    Assert.AreEqual(1, word.Count());

                    Assert.AreEqual("你好", word[0].Word);
                    Assert.AreEqual(1, word[0].UrlFileId);
                    Assert.AreEqual(100, word[0].WordFrequency);

                    word = store.LoadByWord("我们").OrderBy(o => o.UrlFileId).ToList();
                    Assert.AreEqual(2, word.Count());

                    Assert.AreEqual("我们", word[0].Word);
                    Assert.AreEqual(1, word[0].UrlFileId);
                    Assert.AreEqual(200, word[0].WordFrequency);

                    Assert.AreEqual("我们", word[1].Word);
                    Assert.AreEqual(2, word[1].UrlFileId);
                    Assert.AreEqual(300, word[1].WordFrequency);
                }

            }
            finally
            {
                connection.Close();
            }
        }
    }
}