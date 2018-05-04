using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XiaoyaFileParser.Model;
using XiaoyaFileParser.Config;
using XiaoyaStore.Data.Model;
using AngleSharp.Extensions;
using System.Text.RegularExpressions;
using System.Linq;
using XiaoyaNLP.Helper;
using AngleSharp.Dom;

namespace XiaoyaFileParser.Parsers
{
    public class HtmlFileParser : TextFileParser, IFileParser
    {
        private readonly List<string> TagsToRemove = new List<string>
        {
            "script",
            "style",
            "img",
        };

        private readonly List<string> HeaderElementTags = new List<string>
        {
            "h1", "h2", "h3", "h4", "h5"
        };

        private readonly List<string> LinkElementTags = new List<string>
        {
            "a", "img", "area", "span"
        };

        protected HtmlParser mParser = new HtmlParser();

        public HtmlFileParser() : base() { }

        public HtmlFileParser(FileParserConfig config) : base(config) { }

        public override async Task<IList<string>> GetUrlsAsync()
        {
            return (await GetLinksAsync()).Select(o => o.Url).ToList();
        }

        public override async Task<IList<LinkInfo>> GetLinksAsync()
        {
            if (mLinkInfo == null)
            {
                var content = await GetContentAsync();

                mLinkInfo = new List<LinkInfo>();

                var document = await mParser.ParseAsync(content);

                foreach (var tag in LinkElementTags)
                {
                    var links = document.GetElementsByTagName(tag);
                    foreach (var link in links)
                    {
                        try
                        {
                            var href = link.GetAttribute("href");
                            var text = link.Text();
                            if (text == null)
                            {
                                text = "";
                            }
                            if (Uri.TryCreate(new Uri(UrlFile.Url), href, out Uri absoluteUrl))
                            {
                                mLinkInfo.Add(new LinkInfo
                                {
                                    Url = absoluteUrl.ToString(),
                                    Text = TextHelper.ReplaceSpaces(text.ToLower(), ""),
                                });
                            }
                        }
                        catch (Exception)
                        { }
                    }
                }
            }
            return mLinkInfo;
        }

        public override async Task<string> GetTextContentAsync()
        {
            if (mTextContent == null)
            {

                var content = await GetContentAsync();

                var document = await mParser.ParseAsync(content);

                var comments = document.Descendents<IComment>();
                foreach (var element in comments)
                {
                    element.Parent.RemoveChild(element);
                }

                foreach (var tag in TagsToRemove)
                {
                    var elements = document.GetElementsByTagName(tag);
                    foreach (var element in elements)
                    {
                        element.Parent.RemoveChild(element);
                    }
                }

                mTextContent = document.Body.Text();
                if (mTextContent == null)
                {
                    mTextContent = "";
                }
                else
                {
                    mTextContent = TextHelper.RemoveConsecutiveNonsense(TextHelper.ReplaceSpaces(mTextContent));
                }
            }
            return mTextContent;
        }

        public override async Task<string> GetTitleAsync()
        {
            if (mTitle == null)
            {
                var content = await GetContentAsync();

                var document = await mParser.ParseAsync(content);

                mTitle = document.Title;
                if (mTitle == null)
                {
                    mTitle = "";
                }
            }
            return mTitle;
        }

        public override async Task<IList<string>> GetHeadersAsync()
        {
            if (mHeaders == null)
            {
                var content = await GetContentAsync();

                var document = await mParser.ParseAsync(content);

                mHeaders = new List<string>();

                foreach (var tag in HeaderElementTags)
                {
                    foreach (var header in document.GetElementsByTagName(tag))
                    {
                        mHeaders.Add(TextHelper.ReplaceSpaces(header.Text().ToLower(), ""));
                    }
                }
            }
            return mHeaders;
        }
    }
}
