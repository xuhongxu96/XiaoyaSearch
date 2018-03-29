using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.UrlFrontier;
using XiaoyaLogger;
using XiaoyaStore.Data.Model;

namespace XiaoyaCrawler.UrlFrontier
{
    public class SimpleUrlFrontier : IUrlFrontier
    {
        /// <summary>
        /// Is url queue empty
        /// </summary>
        public bool IsEmpty => mConfig.UrlFrontierItemStore.Count() == 0;

        protected RuntimeLogger mLogger;
        protected CrawlerConfig mConfig;
        
        /// <summary>
        /// Sync lock
        /// </summary>
        private object mSyncLock = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Crawler config</param>
        public SimpleUrlFrontier(CrawlerConfig config)
        {
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Crawler.log"));
            mConfig = config;

            if (IsEmpty)
            {
                config.UrlFrontierItemStore.Init(config.InitUrls);
            }
            else
            {
                config.UrlFrontierItemStore.Restart();
            }
        }

        /// <summary>
        /// Pop next url to be fetched
        /// </summary>
        /// <returns>Url to be fetched</returns>
        public UrlFrontierItem PopUrl()
        {
            var item = mConfig.UrlFrontierItemStore.PopUrlForCrawl();

            if (item == null)
            {
                return null;
            }

            mLogger.Log(nameof(SimpleUrlFrontier), "Popped Url: " + item.Url);

            return item;
        }

        /// <summary>
        /// Push a new url
        /// </summary>
        /// <param name="url">New url</param>
        public void PushUrl(string url)
        {
            mConfig.UrlFrontierItemStore.Push(url);
            mLogger.Log(nameof(SimpleUrlFrontier), "Pushed Url: " + url);
        }

        /// <summary>
        /// Push back a popped url
        /// </summary>
        /// <param name="url">Url</param>
        public void PushBackUrl(string url, bool failed = false)
        {
            mConfig.UrlFrontierItemStore.PushBack(url, failed);
            mLogger.Log(nameof(SimpleUrlFrontier), "Pushed Back Url: " + url);
        }
    }
}
