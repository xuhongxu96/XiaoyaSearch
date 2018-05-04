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

        public (string Url, string Content) JudgeContent(UrlFile urlFile)
        {
            var sameFiles = mConfig.UrlFileStore.LoadByHash(urlFile.FileHash).ToList();
            var host = UrlHelper.GetHost(urlFile.Url);

            foreach (var file in sameFiles)
            {
                if (file.url == urlFile.Url)
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

                if (urlFile.TextContent == file.textContent
                    && (currentHost == host || isSameDns))
                {
                    mConfig.SameUrlStore.Save(urlFile.Url, file.url);
                    mLogger.Log(nameof(SimpleSimilarContentManager), $"Find Same UrlFile for {urlFile.Url}: {file.url}");
                    return file;
                }
            }

            return (null, null);
        }
    }
}
