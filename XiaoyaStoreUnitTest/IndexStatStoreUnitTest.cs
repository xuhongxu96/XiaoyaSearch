using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Data;
using XiaoyaStore.Store;

namespace XiaoyaStoreUnitTest
{
    [TestClass]
    public class IndexStatStoreUnitTest
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
                    context.IndexStats.Add(new XiaoyaStore.Data.Model.IndexStat
                    {
                        Word = "你好",
                        WordFrequency = 100,
                        DocumentFrequency = 30,
                    });
                    context.IndexStats.Add(new XiaoyaStore.Data.Model.IndexStat
                    {
                        Word = "我们",
                        WordFrequency = 200,
                        DocumentFrequency = 50,
                    });
                    context.SaveChanges();
                }

                using (var context = new XiaoyaSearchContext(options))
                {
                    var indexStatStore = new IndexStatStore(options);
                    var word1 = indexStatStore.LoadByWord("你好");
                    Assert.AreEqual("你好", word1.Word);
                    Assert.AreEqual(100, word1.WordFrequency);
                    Assert.AreEqual(30, word1.DocumentFrequency);
                    var word2 = indexStatStore.LoadByWord("我们");
                    Assert.AreEqual("我们", word2.Word);
                    Assert.AreEqual(200, word2.WordFrequency);
                    Assert.AreEqual(50, word2.DocumentFrequency);
                }

            }
            finally
            {
                connection.Close();
            }
        }
    }
}