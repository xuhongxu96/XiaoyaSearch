using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaCrawler.UrlFilter;

namespace XiaoyaCrawlerUnitTest
{
    [TestClass]
    public class UrlFilterUnitTest
    {
        [TestMethod]
        public void TestNormalizer()
        {
            var normalizer = new UrlNormalizer();
            var results = normalizer.Filter(new List<string>
            {
                "http://532movie.bnu.edu.cn/index.php?s=video/search/wd/%E9%99%88%E8%B5%AB",
                "http://baidu.com/a?x=1&f=3&x=3&x=2",
                "http://baidu.com/a?x=1,2&f=3&x=3&x=2",
                "http://cas.bnu.edu.cn/cas/login?service=http://532movie.bnu.edu.cn/index.php?s=User/bnulogin"
            });

            foreach (var url in results)
            {
                Console.WriteLine(url);
            }

            Assert.AreEqual(4, results.Count());
        }

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
            Assert.AreEqual(5, results.Count());
        }
    }
}
