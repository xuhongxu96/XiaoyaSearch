﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaFileParser.Config;
using XiaoyaFileParser.Model;
using XiaoyaNLP.Helper;

namespace XiaoyaFileParser.Parsers
{
    public abstract class OfficeParser : BaseParser
    {
        public OfficeParser(FileParserConfig config) : base(config)
        { }

        public OfficeParser()
        { }

        public override async Task<string> GetTextContentAsync()
        {
            if (mTextContent == null)
            {
                var content = await GetContentAsync();

                mTextContent = "";

                if (content.Contains('\n'))
                {
                    var line0 = content.Substring(0, content.IndexOf('\n'));
                    if (int.TryParse(line0, out int contentLength))
                    {
                        mTextContent = content.Substring(line0.Length + 1, contentLength);
                    }
                }
                mTextContent = TextHelper.RemoveConsecutiveNonsense(mTextContent);
            }
            return mTextContent;
        }

        public override async Task<IList<Header>> GetHeadersAsync()
        {
            if (mHeaders == null)
            {
                mHeaders = new List<Header>();

                var content = await GetContentAsync();

                if (content.Contains('\n'))
                {
                    var line0 = content.Substring(0, content.IndexOf('\n'));
                    if (int.TryParse(line0, out int contentLength))
                    {
                        var headerList = content.Substring(line0.Length + 1 + contentLength)
                            .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        for (int i = 0; i < headerList.Count; i += 2)
                        {
                            mHeaders.Add(new Header
                            {
                                Level = int.Parse(headerList[i]),
                                Text = headerList[i + 1],
                            });
                        }
                    }
                }
            }
            return mHeaders;
        }

        public override async Task<string> GetTitleAsync()
        {
            if (mTitle == null)
            {
                var headers = await GetHeadersAsync();

                mTitle = headers.FirstOrDefault()?.Text;
                if (mTitle == null || mTitle == "")
                {
                    mTitle = await base.GetTitleAsync();
                }
            }
            return mTitle;
        }
    }
}
