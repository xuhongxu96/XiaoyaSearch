using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XiaoyaQueryParser.Config;
using XiaoyaQueryParser.QueryParser;
using XiaoyaRetriever;
using XiaoyaRetriever.BooleanRetriever;
using XiaoyaRetriever.Config;
using XiaoyaRetriever.Expression;
using XiaoyaRetriever.InexactTopKRetriever;
using XiaoyaStore.Data;
using XiaoyaStore.Store;

namespace XiaoyaRetrieverUnitTest
{
    [TestClass]
    public class InexactTopKRetrieverUnitTest
    {
        private IEnumerable<string> GetUrls(XiaoyaSearchContext context, IEnumerable<int> urlFileIds)
        {
            foreach (var position in urlFileIds)
            {
                var file = context.UrlFiles.Single(o => o.UrlFileId == position);
                Assert.IsNotNull(file);

                yield return file.Url;
            }
        }


        [TestMethod]
        public void TestRetrieve()
        {
            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                .UseSqlite("Data Source=XiaoyaSearch.db")
                .Options;

            var retriever = new InexactTopKRetriever(new RetrieverConfig
            {
                IndexStatStore = new IndexStatStore(options),
                InvertedIndexStore = new InvertedIndexStore(options),
                UrlFileIndexStatStore = new UrlFileIndexStatStore(options),
                UrlFileStore = new UrlFileStore(options),
            });

            SearchExpression expression = new And
            {
                "����ʦ����ѧ"
            };

            var urlFileIds = retriever.Retrieve(expression);
            Assert.AreEqual(55, urlFileIds.Count());


            expression = new And
            {
                "����ʦ����ѧ",
                new Not(new And
                {
                    "����",
                    "����",
                })
            };

            urlFileIds = retriever.Retrieve(expression);
            Assert.AreEqual(46, urlFileIds.Count());

            using (var context = new XiaoyaSearchContext(options))
            {
                Assert.IsFalse(GetUrls(context, urlFileIds)
                        .Contains("http://jwc.bnu.edu.cn/")
                    );
            }
        }

        [TestMethod]
        public void TestRetrieveWithQueryParser()
        {
            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                .UseSqlite("Data Source=XiaoyaSearch.db")
                .Options;

            var retriever = new InexactTopKRetriever(new RetrieverConfig
            {
                IndexStatStore = new IndexStatStore(options),
                InvertedIndexStore = new InvertedIndexStore(options),
                UrlFileIndexStatStore = new UrlFileIndexStatStore(options),
                UrlFileStore = new UrlFileStore(options),
            });

            var expression = new SimpleQueryParser();

            var urlFileIds = retriever.Retrieve(expression.Parse("����ʦ����ѧ -���񴦽���"));
            Assert.AreEqual(46, urlFileIds.Count());

            using (var context = new XiaoyaSearchContext(options))
            {
                Assert.IsFalse(GetUrls(context, urlFileIds)
                        .Contains("http://jwc.bnu.edu.cn/")
                    );
            }
        }

        [TestMethod]
        public void TestRetrievePerformance()
        {
            const int N = 1000;

            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                .UseSqlite("Data Source=XiaoyaSearch.db")
                .Options;

            using (var context = new XiaoyaSearchContext(options))
            {
                if (context.Database.EnsureCreated())
                {
                    context.Database.ExecuteSqlCommand(File.ReadAllText("init.sql"));
                }
            }

            var retriever = new InexactTopKRetriever(new RetrieverConfig
            {
                IndexStatStore = new IndexStatStore(options),
                InvertedIndexStore = new InvertedIndexStore(options),
                UrlFileIndexStatStore = new UrlFileIndexStatStore(options),
                UrlFileStore = new UrlFileStore(options),
            });

            for (int i = 0; i < N; ++i)
            {
                SearchExpression expression =
                new And
                {
                    "����ʦ����ѧ",
                    "ָ�Ӳ�",
                    "ָ�Ӳ�",
                    "ָ�Ӳ�",
                    "ָ�Ӳ�",
                    "ָ�Ӳ�",
                    new Not(new And
                    {
                        "Эͬ",
                        "����",
                    })
                };

                var urlFileIds = retriever.Retrieve(expression);
            }
        }
    }
}
