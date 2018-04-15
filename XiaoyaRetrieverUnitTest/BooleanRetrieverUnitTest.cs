using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XiaoyaRetriever;
using XiaoyaRetriever.BooleanRetriever;
using XiaoyaRetriever.Config;
using XiaoyaRetriever.Expression;
using XiaoyaStore.Data;
using XiaoyaStore.Store;

namespace XiaoyaRetrieverUnitTest
{
    [TestClass]
    public class BooleanRetrieverUnitTest
    {
        private IEnumerable<string> GetUrls(XiaoyaSearchContext context, IEnumerable<int> urlFileIds)
        {
            foreach (var position in urlFileIds)
            {
                var file = context.UrlFiles.Single(o => o.UrlFileId == position);
                Console.WriteLine(file.Url);

                Assert.IsNotNull(file);

                yield return file.Url;
            }
            Console.WriteLine("---");
        }

        [TestMethod]
        public void TestRetrieve()
        {
            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                .UseSqlite("Data Source=XiaoyaSearch.db")
                .Options;

            var retriever = new BooleanRetriever(new RetrieverConfig
            {
                IndexStatStore = new IndexStatStore(options),
                InvertedIndexStore = new InvertedIndexStore(options),
                UrlFileIndexStatStore = new UrlFileIndexStatStore(options),
                UrlFileStore = new UrlFileStore(options),
            });

            SearchExpression expression = new Or
            {
                new And
                {
                    "ָ�Ӳ�", "����ʦ����ѧ"
                },
                new And
                {
                    "����", "����ʦ����ѧ"
                },
                new And
                {
                    "����ʦ����ѧ", new Not("����ʦ����ѧ")
                }
            };

            var urlFileIds = retriever.Retrieve(expression);

            using (var context = new XiaoyaSearchContext(options))
            {
                Assert.IsTrue(GetUrls(context, urlFileIds)
                        .Contains("http://zhuye.bnu.edu.cn/")
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

            var retriever = new BooleanRetriever(new RetrieverConfig
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
                    new Not("δ��"),
                };

                var urlFileIds = retriever.Retrieve(expression);
            }
        }
    }
}
