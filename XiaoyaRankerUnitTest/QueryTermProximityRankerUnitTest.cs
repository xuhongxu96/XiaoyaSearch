using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using XiaoyaRanker.Config;
using XiaoyaRanker.QueryTermProximityRanker;
using XiaoyaStore.Data;
using XiaoyaStore.Store;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using XiaoyaStore.Data.Model;
using System;

namespace XiaoyaRankerUnitTest
{
    [TestClass]
    public class QueryTermProximityRankerUnitTest
    {
        [TestMethod]
        public void TestRank()
        {
            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                .UseSqlite("Data Source=XiaoyaSearch.db")
                .Options;

            List<UrlFile> urlFiles;

            using (var context = new XiaoyaSearchContext(options))
            {
                urlFiles = context.UrlFiles.ToList();
            }

            var ranker = new QueryTermProximityRanker(new RankerConfig
            {
                UrlFileStore = new UrlFileStore(options),
                IndexStatStore = new IndexStatStore(options),
                UrlFileIndexStatStore = new UrlFileIndexStatStore(options),
                InvertedIndexStore = new InvertedIndexStore(options),
            });

            var scores = ranker.Rank(from urlFile in urlFiles select urlFile.UrlFileId, new List<string>
            {
                "工作",
                "国家政策",
                "财政",
            }).ToList();

            var maxScore = scores.Max();

            for (int i = 0; i < urlFiles.Count(); ++i)
            {
                Console.WriteLine(urlFiles[i].Url);
                Console.WriteLine(scores[i]);
                // Console.WriteLine(urlFiles[i].Content);
                Console.WriteLine("------");

                if (scores[i] == maxScore)
                {
                    Assert.AreEqual("http://news.bnu.edu.cn/sswgh/", urlFiles[i].Url);
                }
            }

            Assert.IsTrue(scores.Any(o => Math.Abs(o - (Math.Log(8) - Math.Log(26))) < double.Epsilon));
            Assert.AreEqual(scores.Count - 1, scores.Count(o => double.IsNegativeInfinity(o)));
        }
    }
}
