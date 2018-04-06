using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using XiaoyaQueryParser.Config;
using XiaoyaRanker.Config;
using XiaoyaRetriever.Config;
using XiaoyaSearch;
using XiaoyaSearch.Config;
using XiaoyaStore.Data;
using XiaoyaStore.Store;

namespace XiaoyaSearchUnitTest
{
    [TestClass]
    public class SearchEngineUnitTest
    {
        [TestMethod]
        public void TestSearch()
        {
            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                .UseSqlite("Data Source=XiaoyaSearch.db")
                .Options;

            var engine = new SearchEngine(new SearchEngineConfig
            {
                IndexStatStore = new IndexStatStore(options),
                UrlFileIndexStatStore = new UrlFileIndexStatStore(options),
                UrlFileStore = new UrlFileStore(options),
                InvertedIndexStore = new InvertedIndexStore(options),
            });

            var results = engine.Search("教务处");
            var store = new UrlFileStore(options);

            foreach (var result in results)
            {
                var urlFile = store.LoadById(result.UrlFileId);

                Console.WriteLine("{0}: {1} ({2})", result.UrlFileId, urlFile.Url, result.Score);
            }

        }

        [TestMethod]
        public void TestSearchPerformance()
        {
            const int N = 100;

            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                .UseSqlite("Data Source=XiaoyaSearch.db")
                .Options;

            var engine = new SearchEngine(new SearchEngineConfig
            {
                IndexStatStore = new IndexStatStore(options),
                UrlFileIndexStatStore = new UrlFileIndexStatStore(options),
                UrlFileStore = new UrlFileStore(options),
                InvertedIndexStore = new InvertedIndexStore(options),
            });

            for (int i = 0; i < N; ++i)
            {
                var results = engine.Search("北京师范大学教务处");
                var store = new UrlFileStore(options);

                foreach (var result in results)
                {
                    var urlFile = store.LoadById(result.UrlFileId);

                    Console.WriteLine("{0}: {1} ({2})", result.UrlFileId, urlFile.Url, result.Score);
                }

                Console.WriteLine("---");
            }
        }
    }
}
