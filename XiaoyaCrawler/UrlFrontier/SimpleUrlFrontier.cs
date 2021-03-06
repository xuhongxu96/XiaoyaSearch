﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XiaoyaCrawler.Config;
using XiaoyaCrawler.UrlFrontier;
using XiaoyaLogger;

namespace XiaoyaCrawler.UrlFrontier
{
    public class SimpleUrlFrontier : IUrlFrontier
    {
        protected RuntimeLogger mLogger;
        protected CrawlerConfig mConfig;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Crawler config</param>
        public SimpleUrlFrontier(CrawlerConfig config)
        {
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Crawler.log"));
            mConfig = config;
            config.UrlFrontierItemStore.Reload();
            config.UrlFrontierItemStore.Init(config.InitUrls);
        }

        /// <summary>
        /// Pop next url to be fetched
        /// </summary>
        /// <returns>Url to be fetched</returns>
        public string PopUrl()
        {
            var url = mConfig.UrlFrontierItemStore.PopUrl();

            if (url == null)
            {
                return null;
            }

            mLogger.Log(nameof(SimpleUrlFrontier), "Popped Url: " + url);

            return url;
        }

        /// <summary>
        /// Push a new url
        /// </summary>
        /// <param name="url">New url</param>
        public void PushUrls(IEnumerable<string> urls)
        {
            mConfig.UrlFrontierItemStore.PushUrls(urls);
            // mLogger.Log(nameof(SimpleUrlFrontier), "Pushed Urls:\n\n\t" + string.Join("\n\t", urls) + "\n");
        }

        /// <summary>
        /// Push back a popped url
        /// </summary>
        /// <param name="url">Url</param>
        public void PushBackUrl(string url, ulong updateInterval, bool failed = false)
        {
            mConfig.UrlFrontierItemStore.PushBackUrl(url, updateInterval, failed);
            mLogger.Log(nameof(SimpleUrlFrontier), "Pushed Back Url: " + url);
        }

        public void RemoveUrl(string url)
        {
            mConfig.UrlFrontierItemStore.RemoveUrl(url);
            mLogger.Log(nameof(SimpleUrlFrontier), "Removed Url: " + url);
        }
    }
}
