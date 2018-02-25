using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;
using XiaoyaCrawler.Config;

namespace XiaoyaCrawler.Parser
{
    /// <summary>
    /// Web content parser
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Parse a url file
        /// </summary>
        /// <param name="urlFile">Url file</param>
        /// <returns>Parsing result containing text content and urls in file
        /// Returns null if cannot parse the file</returns>
        Task<ParseResult> ParseAsync(UrlFile urlFile);
    }
}
