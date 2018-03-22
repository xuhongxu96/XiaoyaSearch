﻿using System;
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
            var mParser = new UniversalFileParser
            {
                UrlFile = urlFile
            };
            
            return new ParseResult
            {
                Content = await mParser.GetTextContentAsync(),
                Urls = await mParser.GetUrlsAsync()
            };
        }
    }
}
