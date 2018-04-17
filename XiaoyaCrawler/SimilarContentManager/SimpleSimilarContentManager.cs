using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Helper;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.Parser;
using XiaoyaLogger;
using XiaoyaStore.Data.Model;

namespace XiaoyaCrawler.SimilarContentManager
{
    public class SimpleSimilarContentManager : ISimilarContentManager
    {

        protected CrawlerConfig mConfig;
        protected RuntimeLogger mLogger;

        public SimpleSimilarContentManager(CrawlerConfig config)
        {
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Crawler.Log"), true);
            mConfig = config;
        }

        public UrlFile JudgeContent(UrlFile urlFile)
        {
            var sameFiles = mConfig.UrlFileStore.LoadByHash(urlFile.FileHash);
            var host = UrlHelper.GetHost(urlFile.Url);

            foreach (var file in sameFiles)
            {
                if (urlFile.Content == file.Content && UrlHelper.GetHost(file.Url) == host)
                {
                    mLogger.Log(nameof(SimpleSimilarContentManager), $"Find Same UrlFile for {urlFile.Url}: {file.Url}");
                    return file;
                }
            }

            return null;
        }
    }
}
