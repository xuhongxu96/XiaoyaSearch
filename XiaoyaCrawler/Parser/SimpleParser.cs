using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Helper;
using XiaoyaCrawler.Config;
using XiaoyaFileParser;
using XiaoyaLogger;

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
            IFileParser parser = new UniversalFileParser
            {
                UrlFile = urlFile,
                FilePath = urlFile.FilePath,
            };

            var content = await parser.GetContentAsync();

            if (content.Trim() == "")
            {
                throw new InvalidDataException("Empty content");
            }

            return new ParseResult
            {
                Title = await parser.GetTitleAsync(),
                Content = content,
                TextContent = await parser.GetTextContentAsync(),
                Links = await parser.GetLinksAsync(),
                PublishDate = await parser.GetPublishDateAsync(),
            };
        }
    }
}
