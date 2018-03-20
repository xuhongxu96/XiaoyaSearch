using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaCrawler.UrlFilter;

namespace XiaoyaCrawlerUnitTest
{
    [TestClass]
    public class DomainUrlFilterUnitTest
    {
        [TestMethod]
        public void TestFilter()
        {
            var filter = new DomainUrlFilter(@"(bnu\.edu\.cn)|(//172\.)");
            var results = filter.Filter(new List<string>
            {
                "http://www.bnu.edu.cn/",
                "http://www.bnu.edu.cn/news",
                "http://bnu.edu.cn/news",
                "http://www.bnu.edu.com/",
                "http://172.16.202.201",
                "https://172.11.1.1/22",
                "http://192.168.1.1",
                "http://192.168.172.1"
            });
            Assert.AreEqual(5, results.Count);
        }
    }
}
