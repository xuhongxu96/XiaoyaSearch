using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Helper;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.Parser;
using XiaoyaLogger;

namespace XiaoyaCrawler.SimilarContentManager
{
    public class SimpleSimilarContentManager : ISimilarContentManager
    {

        protected CrawlerConfig mConfig;
        protected RuntimeLogger mLogger;

        public SimpleSimilarContentManager(CrawlerConfig config)
        {
            mLogger = new RuntimeLogger(
                Path.Combine(config.LogDirectory, "SimpleSimilarContentManager.Log"));
            mConfig = config;
        }

        public async void AddContentAsync(string url, string content)
        {
            await Task.Run(() =>
            {
            });
        }
    }
}
