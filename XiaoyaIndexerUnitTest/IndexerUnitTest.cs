using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XiaoyaIndexer;
using XiaoyaIndexer.Config;
using XiaoyaStore.Data;
using XiaoyaStore.Store;

namespace XiaoyaIndexerUnitTest
{
    [TestClass]
    public class IndexerUnitTest
    {
        private string logDir = Path.Combine(Path.GetTempPath(), "Logs");
        private string fetchDir = Path.Combine(Path.GetTempPath(), "Fetched");

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

            Directory.CreateDirectory(fetchDir);
            foreach (var file in Directory.EnumerateFiles("Fetched"))
            {
                File.Copy(file, Path.Combine(fetchDir, Path.GetFileName(file)), true);
            }

            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                .UseSqlite("Data Source=XiaoyaSearch.db")
                .Options;

            var config = new IndexerConfig
            {
                LogDirectory = logDir,
                UrlFileStore = new UrlFileStore(options),
                InvertedIndexStore = new InvertedIndexStore(options),
            };

            using (var context = new XiaoyaSearchContext(options))
            {
                if (context.Database.EnsureCreated())
                {
                    context.Database.ExecuteSqlCommand(File.ReadAllText("init.sql"));
                }

                foreach (var urlFile in context.UrlFiles)
                {
                    urlFile.IsIndexed = false;
                }

                context.RemoveRange(context.InvertedIndices);
                context.RemoveRange(context.IndexStats);
                context.RemoveRange(context.UrlFileIndexStats);
                context.SaveChanges();
            }

            IIndexer indexer = new SimpleIndexer(config);

            Task.Run(() =>
            {
                indexer.CreateIndexAsync().GetAwaiter().GetResult();
            });

            while (!indexer.IsWaiting)
            {
                Thread.Sleep(1000);
            }

            indexer.StopIndex();

            using (var context = new XiaoyaSearchContext(options))
            {
                foreach (var urlFile in context.UrlFiles)
                {
                    Assert.IsTrue(urlFile.IsIndexed);

                    var id = urlFile.UrlFileId;
                    if (urlFile.Content.Trim() != "")
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
