using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        /// Push a new url
        /// </summary>
        /// <param name="url">New url</param>
        void PushUrl(string url);
        /// <summary>
        /// Load check point to recover progress
        /// </summary>
        Task LoadCheckPoint();
        /// <summary>
        /// Save check point to recover progress
        /// </summary>
        Task SaveCheckPoint();
        /// <summary>
        /// Is no url to be fetched
        /// </summary>
        bool IsEmpty { get; }
    }
}
