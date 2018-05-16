using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;
using XiaoyaCrawler.Config;
using XiaoyaLogger;
using XiaoyaCrawler.Fetcher;

namespace XiaoyaCrawler.Parser
{
    /// <summary>
    /// Web content parser
    /// </summary>
    public interface IParser
    {
        Task<ParseResult> ParseAsync(FetchedFile fetchedFile);
    }
}
