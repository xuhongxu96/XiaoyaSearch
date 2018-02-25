using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XiaoyaFileParser.Model;
using XiaoyaFileParser.Config;
using XiaoyaStore.Data.Model;

namespace XiaoyaFileParser
{
    public class HtmlFileParser : TextFileParser
    {
        protected HtmlParser mParser = new HtmlParser();
        protected string mTextContent = null;

        public HtmlFileParser() : base() { }

        public HtmlFileParser(FileParserConfig config) : base(config) { }

        public override async Task<IList<string>> GetUrlsAsync()
        {
            var content = await GetContentAsync();

            var result = new List<string>();

            var document = await mParser.ParseAsync(content);
            var links = document.GetElementsByTagName("a");
            foreach (var link in links)
            {
                var href = link.GetAttribute("href");
                var absoluteUrl = new Uri(new Uri(UrlFile.Url), href);
                result.Add(absoluteUrl.ToString());
            }

            return result;
        }

        public override async Task<string> GetTextContentAsync()
        {
            if (mTextContent == null)
            {
                var content = await GetContentAsync();

                var document = await mParser.ParseAsync(content);
                mTextContent = document.TextContent;
            }
            return mTextContent;
        }
    }
}
