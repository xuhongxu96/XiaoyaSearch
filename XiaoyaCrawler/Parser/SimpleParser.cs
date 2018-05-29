using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Helper;
using XiaoyaCrawler.Config;
using XiaoyaFileParser;
using XiaoyaLogger;
using XiaoyaCrawler.Fetcher;

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
        /// Parse a fetched file
        /// </summary>
        /// <param name="fetchedFile">Fetched file</param>
        /// <returns>Parsing result containing text content and urls in file
        /// Returns null if cannot parse the file</returns>
        public async Task<ParseResult> ParseAsync(FetchedFile fetchedFile)
        {
            if (fetchedFile == null)
            {
                throw new ArgumentNullException(nameof(fetchedFile));
            }

            IFileParser parser = new UniversalFileParser();
            parser.SetFile(fetchedFile.MimeType, fetchedFile.Url, fetchedFile.Charset, fetchedFile.FilePath);

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
