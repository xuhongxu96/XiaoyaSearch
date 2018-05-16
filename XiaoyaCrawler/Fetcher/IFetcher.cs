using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaLogger;
using XiaoyaStore.Data.Model;

namespace XiaoyaCrawler.Fetcher
{
    public interface IFetcher
    {
        /// <summary>
        /// Fetch the web content in the specific url
        /// </summary>
        /// <param name="url">Url in which to fetch the content</param>
        /// <returns>Local url to downloaded content</returns>
        Task<FetchedFile> FetchAsync(string url);
    }
}
