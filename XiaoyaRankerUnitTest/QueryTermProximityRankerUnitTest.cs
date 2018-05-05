using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using XiaoyaRanker.Config;
using XiaoyaStore.Data;
using XiaoyaStore.Store;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using XiaoyaStore.Data.Model;
using System;
using XiaoyaRanker.PositionRanker.QueryTermProximityRanker;

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
                InvertedIndexStore = new InvertedIndexStore(options),
            });

            var results = ranker.Rank(from urlFile in urlFiles select urlFile.UrlFileId, new List<string>
            {
                "孔子",
                "学院",
                "奖学金",
            }).ToList();

            var scores = results.Select(o => o.Value).ToList();

            var maxScore = scores.Max();

            Assert.AreEqual(1, maxScore);

            for (int i = 0; i < urlFiles.Count(); ++i)
            {
                Console.WriteLine(urlFiles[i].Url);
                Console.WriteLine(scores[i]);
                if (results[i].WordPositions != null)
                {
                    foreach (var wordPos in results[i].WordPositions)
                    {
                        Console.Write(wordPos.Word + " (" + wordPos.Position + ")  |  ");
                    }
                    Console.WriteLine();
                }

                Console.WriteLine("------");

                if (scores[i] == maxScore)
                {
                    Assert.AreEqual("http://ocia.bnu.edu.cn/", urlFiles[i].Url);
                }
            }
        }
    }
}
