using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XiaoyaCrawler.UrlFilter
{
    /// <summary>
    /// Url filter
    /// To filter out urls that should not be fetched
    /// </summary>
    public interface IUrlFilter
    {
        /// <summary>
        /// Filter urls
        /// </summary>
        /// <param name="urls">Urls to be filtered</param>
        /// <returns>Filtered urls</returns>
        IList<string> Filter(IList<string> urls);
        /// <summary>
        /// Load check point to recover progress
        /// </summary>
        Task LoadCheckPoint();
        /// <summary>
        /// Save check point to recover progress
        /// </summary>
        Task SaveCheckPoint();
    }
}
