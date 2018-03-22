using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XiaoyaRetriever;
using XiaoyaRetriever.BooleanRetriever;
using XiaoyaRetriever.BooleanRetriever.Expression;
using XiaoyaRetriever.Config;
using XiaoyaStore.Data;
using XiaoyaStore.Store;

namespace XiaoyaRetrieverUnitTest
{
    [TestClass]
    public class BooleanRetrieverUnitTest
    {
        private IEnumerable<string> OutputPositionAndGetUrls(XiaoyaSearchContext context, IEnumerable<RetrievedUrlFilePositions> positions)
        {
            Console.WriteLine("----------");
            foreach (var position in positions)
            {
                var file = context.UrlFiles.Single(o => o.UrlFileId == position.UrlFileId);
                Assert.IsNotNull(file);

                yield return file.Url;

                Console.WriteLine(position.UrlFileId + ": " + file.Url);
                foreach (var p in position)
                {
                    Console.Write(p.Position + " (" + p.Word + "), ");
                }
                Console.Write("\n\n");

                var content = file.Content;
                var length = content.Length;

                int start = -1, end = -1;

                foreach (var word in position.OrderBy(o => o.Position))
                {
                    var curStart = Math.Max(0, word.Position - 10);
                    var curEnd = Math.Min(length, word.Position + 10);

                    if (curStart - 3 <= end)
                    {
                        end = curEnd;
                    }
                    else
                    {
                        if (start != -1)
                        {
                            Console.WriteLine(content.Substring(start, end - start).Replace("\n", " ").Trim());
                        }

                        start = curStart;
                        end = curEnd;
                    }
                }
                if (start != -1)
                {
                    Console.WriteLine(content.Substring(start, end - start).Replace("\n", " ").Trim());
                }

                Console.WriteLine();
            }
        }


        [TestMethod]
        public void TestRetrieve()
        {
            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                .UseSqlite("Data Source=XiaoyaSearch.db")
                .Options;

            using (var context = new XiaoyaSearchContext(options))
            {
                context.Database.EnsureCreated();

                context.RemoveRange(context.UrlFiles);
                context.RemoveRange(context.InvertedIndices);
                context.RemoveRange(context.IndexStats);
                context.SaveChanges();

                context.Database.ExecuteSqlCommand(File.ReadAllText("init.sql"));
            }

            var retriever = new BooleanRetriever(new RetrieverConfig
            {
                IndexStatStore = new IndexStatStore(options),
                InvertedIndexStore = new InvertedIndexStore(options),
            });

            Expression expression = new Or
            {
                new And
                {
                    "指挥部", "北京师范大学"
                },
                new And
                {
                    "协同", "未来"
                }
            };

            var positions = retriever.Retrieve(expression);
            Assert.AreEqual(5, positions.Count());

            using (var context = new XiaoyaSearchContext(options))
            {
                Assert.IsTrue(OutputPositionAndGetUrls(context, positions)
                        .Contains("http://www.bnu.edu.cn/kxyj/")
                    );
            }


            expression = new And
            {
                "北京师范大学",
                new Not("协同"),
            };

            positions = retriever.Retrieve(expression);
            Assert.AreEqual(11, positions.Count());

            using (var context = new XiaoyaSearchContext(options))
            {
                Assert.IsFalse(OutputPositionAndGetUrls(context, positions)
                        .Contains("http://www.bnu.edu.cn/kxyj/")
                    );
            }


        }
    }
}
