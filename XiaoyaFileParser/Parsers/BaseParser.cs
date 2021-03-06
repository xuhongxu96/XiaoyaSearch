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
    public abstract class BaseParser : IFileParser
    {
        protected string mFilePath = null;
        protected string mMimeType;
        protected string mCharset;
        protected string mUrl;

        public void SetFile(string mimeType, string url, string charset, string filePath = null,
            string content = null, string textContent = null, string title = null)
        {
            mMimeType = mimeType;
            mUrl = url;
            mCharset = charset;
            mFilePath = filePath;

            mTitle = title;
            mContent = content;
            mTextContent = textContent;
        }

        protected FileParserConfig mConfig = new FileParserConfig();
        protected string mTitle = null;
        protected string mContent = null;
        protected string mTextContent = null;
        protected List<LinkInfo> mLinkInfo = null;
        protected List<Header> mHeaders = null;
        protected DateTime mPublishDate = DateTime.MinValue;

        public BaseParser() { }

        public BaseParser(FileParserConfig config)
        {
            mConfig = config;
        }

        public virtual async Task<IList<Token>> GetTokensAsync()
        {
            var textContent = await GetTextContentAsync();

            var title = await GetTitleAsync();
            var headers = await GetHeadersAsync();

            var result = new List<Token>();
            var wordDict = new Dictionary<string, Token>();

            foreach (var segment in mConfig.TextSegmenter.Segment(textContent).GroupBy(o => TextHelper.NormalizeIndexWord(o.Word)))
            {
                var token = new Token
                {
                    Word = segment.Key,
                    Positions = segment.Select(o => o.Position).OrderBy(o => o).ToList(),
                    Length = (uint) segment.Key.Length,
                    WordFrequency = (uint) segment.Count(),
                    OccurencesInLinks = 0,
                    OccurencesInTitle = 0,
                    OccurencesInHeaders = 0,
                };
                result.Add(token);
                wordDict.Add(token.Word, token);
            }

            foreach (var segment in mConfig.TextSegmenter.Segment(title).GroupBy(o => TextHelper.NormalizeIndexWord(o.Word)))
            {
                if (wordDict.ContainsKey(segment.Key))
                {
                    wordDict[segment.Key].OccurencesInTitle = (uint) segment.Count();
                    wordDict[segment.Key].WordFrequency += (uint) segment.Count();
                }
                else
                {
                    var token = new Token
                    {
                        Word = segment.Key,
                        Positions = new List<uint>(),
                        Length = (uint) segment.Key.Length,
                        WordFrequency = (uint) segment.Count(),
                        OccurencesInLinks = 0,
                        OccurencesInTitle = (uint) segment.Count(),
                        OccurencesInHeaders = 0,
                    };
                    result.Add(token);
                    wordDict.Add(token.Word, token);
                }
            }

            foreach (var header in headers)
            {
                foreach (var segment in mConfig.TextSegmenter.Segment(header.Text).GroupBy(o => TextHelper.NormalizeIndexWord(o.Word)))
                {
                    if (wordDict.ContainsKey(segment.Key))
                    {
                        wordDict[segment.Key].OccurencesInHeaders = (uint) (segment.Count() * (6 - header.Level));
                    }
                    else
                    {
                        var token = new Token
                        {
                            Word = segment.Key,
                            Positions = new List<uint>(),
                            Length = (uint) segment.Key.Length,
                            WordFrequency = (uint) segment.Count(),
                            OccurencesInLinks = 0,
                            OccurencesInTitle = 0,
                            OccurencesInHeaders = (uint) (segment.Count() * (6 - header.Level)),
                        };
                        result.Add(token);
                        wordDict.Add(token.Word, token);
                    }
                }
            }

            return result;
        }

        public virtual async Task<IList<Token>> GetTokensAsync(IEnumerable<string> linkTexts)
        {
            var result = await GetTokensAsync();
            var wordDict = result.ToDictionary(o => o.Word);

            foreach (var link in linkTexts)
            {
                foreach (var segment in mConfig.TextSegmenter.Segment(link).GroupBy(o => TextHelper.NormalizeIndexWord(o.Word)))
                {
                    if (wordDict.ContainsKey(segment.Key))
                    {
                        wordDict[segment.Key].OccurencesInLinks += (uint) segment.Count();
                        wordDict[segment.Key].WordFrequency += (uint) segment.Count();
                    }
                    else
                    {
                        var token = new Token
                        {
                            Word = segment.Key,
                            Positions = new List<uint>(),
                            Length = (uint) segment.Key.Length,
                            WordFrequency = (uint) segment.Count(),
                            OccurencesInLinks = (uint) segment.Count(),
                            OccurencesInTitle = 0,
                            OccurencesInHeaders = 0,
                        };
                        result.Add(token);
                        wordDict.Add(token.Word, token);
                    }
                }
            }

            return result;
        }

        public virtual async Task<string> GetTextContentAsync()
        {
            if (mTextContent == null)
            {
                mTextContent = await GetContentAsync();
            }
            return mTextContent;
        }

        public virtual async Task<IList<string>> GetUrlsAsync()
        {
            return await Task.Run(() => new List<string>());
        }

        public virtual async Task<IList<LinkInfo>> GetLinksAsync()
        {
            return await Task.Run(() => new List<LinkInfo>());
        }

        public virtual async Task<string> GetTitleAsync()
        {
            if (mTitle == null)
            {
                var content = await GetTextContentAsync();

                await Task.Run(() =>
                {
                    var lines = content.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    mTitle = lines.FirstOrDefault();
                    if (mTitle == null)
                    {
                        mTitle = "";
                    }
                    else if (mTitle.Length < 4 && lines.Length > 1)
                    {
                        mTitle += "  " + lines[1];
                    }
                });
            }
            return mTitle;
        }

        public async Task<DateTime> GetPublishDateAsync()
        {
            if (mPublishDate == DateTime.MinValue)
            {
                var content = await GetContentAsync();

                var dates = TextHelper.ExtractDateTime(content);
                if (dates.Any())
                {
                    mPublishDate = dates.First();
                    if (mPublishDate > DateTime.Now)
                    {
                        mPublishDate = DateTime.Now;
                    }
                }
                else
                {
                    mPublishDate = DateTime.Now;
                }
            }
            return mPublishDate;
        }

        public abstract Task<string> GetContentAsync();

        public virtual async Task<IList<Header>> GetHeadersAsync()
        {
            return await Task.Run(() =>
            {
                return new List<Header>();
            });
        }
    }
}
