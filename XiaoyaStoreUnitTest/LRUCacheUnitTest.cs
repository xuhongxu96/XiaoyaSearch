using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Cache;

namespace XiaoyaStoreUnitTest
{
    [TestClass]
    public class LRUCacheUnitTest
    {
        [TestMethod]
        public void TestCache()
        {
            var cache = new LRUCache<string, string>(TimeSpan.FromDays(1), GetCache, null, 3);

            cache.Get("a");
            cache.Get("b");
            cache.Get("c");

            Assert.IsTrue(cache.IsValid("a"));
            Assert.IsTrue(cache.IsValid("b"));
            Assert.IsTrue(cache.IsValid("c"));

            cache.Get("d");

            Assert.IsTrue(cache.IsValid("b"));
            Assert.IsTrue(cache.IsValid("c"));
            Assert.IsTrue(cache.IsValid("d"));

            cache.Get("b");

            Assert.IsTrue(cache.IsValid("c"));
            Assert.IsTrue(cache.IsValid("d"));
            Assert.IsTrue(cache.IsValid("b"));

            cache.Get("a");

            Assert.IsTrue(cache.IsValid("d"));
            Assert.IsTrue(cache.IsValid("b"));
            Assert.IsTrue(cache.IsValid("a"));
        }

        [TestMethod]
        public void TestConcurrentCache()
        {
            const int N = 10000, M = 400;
            var cache = new LRUCache<string, string>(TimeSpan.FromDays(1), GetCache, null, 3);

            string[] strs = { "a", "b", "c", "d" };
            var tasks = new List<Task>();

            for (int i = 0; i < N; ++i)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < M; ++j)
                    {
                        cache.Get(strs[j % 4]);
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());

            Assert.IsTrue(cache.IsValid("b"));
            Assert.IsTrue(cache.IsValid("c"));
            Assert.IsTrue(cache.IsValid("d"));
        }

        protected string GetCache(string key)
        {
            return key.ToUpper();
        }
    }
}
