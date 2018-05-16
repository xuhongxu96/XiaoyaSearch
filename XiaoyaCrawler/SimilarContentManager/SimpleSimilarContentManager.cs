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

        public (string Url, string Content) JudgeContent(FetchedFile fetchedFile, string textContent)
        {
            var sameFiles = mConfig.UrlFileStore.LoadByHash(fetchedFile.fileHash).ToList();
            var host = UrlHelper.GetHost(fetchedFile.url);

            foreach (var file in sameFiles)
            {
                if (file.url == fetchedFile.url)
                {
                    continue;
                }

                var currentHost = UrlHelper.GetHost(file.url);

                bool isSameDns = false;

                try
                {
                    isSameDns = Dns.GetHostAddresses(currentHost).SequenceEqual(Dns.GetHostAddresses(host));
                }
                catch (Exception)
                { }

                if (textContent == file.textContent
                    && (currentHost == host || isSameDns))
                {
                    mLogger.Log(nameof(SimpleSimilarContentManager), $"Find Same UrlFile for {fetchedFile.url}: {file.url}");
                    return file;
                }
            }

            return (null, null);
        }
    }
}
