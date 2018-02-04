using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XiaoyaCommon.Helper;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.Parser;

namespace XiaoyaCrawler.SimilarContentJudger
{
    public class SimpleSimilarContentJudger : ISimilarContentJudger
    {

        protected CrawlerConfig mConfig;

        public SimpleSimilarContentJudger(CrawlerConfig config)
        {
            mConfig = config;
        }

        public async void AddContentAsync(string url, string content)
        {
            await Task.Run(() =>
            {
            });
        }

        public async Task LoadCheckPoint()
        {
            return;
        }

        public async Task SaveCheckPoint()
        {
            return;
        }
    }
}
