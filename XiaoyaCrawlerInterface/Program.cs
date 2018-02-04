using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XiaoyaCommon.Store;
using XiaoyaCrawler;

namespace XiaoyaCrawlerInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            var crawler = new Crawler(new XiaoyaCrawler.Config.CrawlerConfig
            {
                InitUrls = new List<string>
                {
                    "http://www.bnu.edu.cn",
                },
                UrlFileStore = new UrlFileStore(),
            });
            crawler.StartAsync().GetAwaiter().GetResult();
        }
    }
}
