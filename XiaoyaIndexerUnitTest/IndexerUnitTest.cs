using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XiaoyaIndexer;
using XiaoyaIndexer.Config;
using XiaoyaNLP.Helper;
using XiaoyaStore.Data;
using XiaoyaStore.Store;
using static XiaoyaStore.Data.Model.UrlFile;

namespace XiaoyaIndexerUnitTest
{
    [TestClass]
    public class IndexerUnitTest
    {
        private string logDir = Path.Combine(Path.GetTempPath(), "Logs");

        [TestMethod]
        public void TestIndex()
        {
            while (true)
            {
                try
                {
                    if (Directory.Exists(logDir))
                        Directory.Delete(logDir, true);
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(500);
                }
            }

            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                .UseSqlite("Data Source=XiaoyaSearch.db")
                .Options;

            using (var context = new XiaoyaSearchContext(options))
            {
                context.RemoveRange(context.InvertedIndices);
                context.RemoveRange(context.UrlFileIndexStats);
                context.RemoveRange(context.IndexStats);
                context.SaveChanges();
            }

            var config = new IndexerConfig
            {
                LogDirectory = logDir,
                UrlFileStore = new UrlFileStore(options),
                InvertedIndexStore = new InvertedIndexStore(options),
            };

            IIndexer indexer = new SimpleIndexer(config);

            Task.Run(() =>
            {
                indexer.CreateIndexAsync().GetAwaiter().GetResult();
            });

            while (!indexer.IsWaiting)
            {
                Thread.Sleep(1000);
            }

            indexer.WaitAll();

            indexer.StopIndex();

            using (var context = new XiaoyaSearchContext(options))
            {
                foreach (var urlFile in context.UrlFiles)
                {
                    if (urlFile.Url == "http://media.bnu.edu.cn/")
                    {
                        continue;
                    }
                    Assert.IsTrue(urlFile.IndexStatus == UrlFileIndexStatus.Indexed);

                    var id = urlFile.UrlFileId;
                    if (urlFile.Content.Trim() != ""
                        && CommonRegex.RegexAllChars.IsMatch(urlFile.Content.Trim()))
                    {
                        var indices = context.InvertedIndices
                            .Where(o => o.UrlFileId == id);

                        Assert.AreNotEqual(0, indices.Count());
                    }
                }
            }
        }
    }
}
