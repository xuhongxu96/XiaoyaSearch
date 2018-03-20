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
        UrlFrontierItem PopUrl();
        /// <summary>
        /// Push a new url
        /// </summary>
        /// <param name="url">New url</param>
        void PushUrl(string url);
        /// <summary>
        /// Push back a popped url
        /// </summary>
        /// <param name="url">Url</param>
        void PushBackUrl(string url);
        /// <summary>
        /// Is no url to be fetched
        /// </summary>
        bool IsEmpty { get; }
    }
}
