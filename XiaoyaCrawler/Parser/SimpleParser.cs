using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XiaoyaCommon.Data.Crawler.Model;
using XiaoyaCommon.Helper;
using XiaoyaCrawler.Config;

namespace XiaoyaCrawler.Parser
{
    public class SimpleParser : IParser
    {
        protected CrawlerConfig mConfig;

        public SimpleParser(CrawlerConfig config)
        {
            mConfig = config;
        }

        /// <summary>
        /// Parse a url file
        /// </summary>
        /// <param name="urlFile">Url file</param>
        /// <returns>Parsing result containing text content and urls in file
        /// Returns null if cannot parse the file</returns>
        public async Task<ParseResult> ParseAsync(UrlFile urlFile)
        {
            if (urlFile.MimeType == "text/html")
            {
                var content = await File.ReadAllTextAsync(urlFile.FilePath);
                return new ParseResult
                {
                    Content = content,
                    Urls = await HtmlHelper.GetUrlsAsync(content, urlFile.Url)
                };
            }
            return null;
        }
    }
}
