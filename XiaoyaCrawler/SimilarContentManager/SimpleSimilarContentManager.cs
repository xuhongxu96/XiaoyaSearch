using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Helper;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.Parser;
using XiaoyaLogger;
using System.Net;
using System.Linq;
using XiaoyaCrawler.Fetcher;

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

        public (string Url, string Content) JudgeContent(FetchedFile fetchedFile, string content)
        {
            var sameFiles = mConfig.UrlFileStore.GetUrlFilesByHash(fetchedFile.FileHash);
            var host = UrlHelper.GetHost(fetchedFile.Url);

            foreach (var file in sameFiles)
            {
                if (file.Url == fetchedFile.Url)
                {
                    continue;
                }

                var currentHost = UrlHelper.GetHost(file.Url);

                bool isSameDns = false;

                try
                {
                    isSameDns = Dns.GetHostAddresses(currentHost).SequenceEqual(Dns.GetHostAddresses(host));
                }
                catch (Exception)
                { }

                if (content == file.Content
                    && (currentHost == host || isSameDns))
                {
                    mLogger.Log(nameof(SimpleSimilarContentManager), $"Find Same UrlFile for {fetchedFile.Url}: {file.Url}");
                    return (file.Url, file.Content);
                }
            }

            return (null, null);
        }
    }
}
