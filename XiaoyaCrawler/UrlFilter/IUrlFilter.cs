using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaFileParser.Model;
using XiaoyaStore.Data.Model;

namespace XiaoyaCrawler.UrlFilter
{
    /// <summary>
    /// Url filter
    /// To filter out urls that should not be fetched
    /// </summary>
    public interface IUrlFilter
    {
        /// <summary>
        /// Filter urls of links
        /// </summary>
        /// <param name="links">Links to be filtered</param>
        /// <returns>Filtered urls</returns>
        IEnumerable<LinkInfo> Filter(IEnumerable<LinkInfo> links);
    }
}
