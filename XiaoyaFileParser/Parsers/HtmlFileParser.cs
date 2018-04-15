﻿using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XiaoyaFileParser.Model;
using XiaoyaFileParser.Config;
using XiaoyaStore.Data.Model;
using AngleSharp.Extensions;
using System.Text.RegularExpressions;
using XiaoyaCommon.Helper;
using System.Linq;

namespace XiaoyaFileParser.Parsers
{
    public class HtmlFileParser : TextFileParser
    {
        static readonly Regex sTrimmer = new Regex(@"\s\s+");

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
                var links = document.GetElementsByTagName("a");
                foreach (var link in links)
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
                            Text = text,
                        });
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

                var scripts = document.GetElementsByTagName("script");

                foreach (var script in scripts)
                {
                    script.Parent.RemoveChild(script);
                }

                mTextContent = document.Body.Text();
                if (mTextContent == null)
                {
                    mTextContent = "";
                }
                else
                {
                    mTextContent = sTrimmer.Replace(mTextContent.Trim(), "\n");
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
    }
}
