using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;

namespace XiaoyaCrawler.UrlFrontier
{
    /// <summary>
    /// Url Frontier
    /// Containing URLs yet to be fetched in the current crawl
    /// </summary>
    public interface IUrlFrontier
    {
        /// <summary>
        /// Pop next url to be fetched
        /// </summary>
        /// <returns>Url to be fetched</returns>
        string PopUrl();
        /// <summary>
        /// Push new urls
        /// </summary>
        /// <param name="urls">New urls</param>
        void PushUrls(IEnumerable<string> urls);
        /// <summary>
        /// Push back a popped url
        /// </summary>
        /// <param name="url">Url</param>
        void PushBackUrl(string url, bool failed = false);
        /// <summary>
        /// Remove Url
        /// </summary>
        /// <param name="url">Url</param>
        void RemoveUrl(string url);
    }
}
