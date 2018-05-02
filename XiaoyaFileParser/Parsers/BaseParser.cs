using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaFileParser.Config;
using XiaoyaFileParser.Model;
using XiaoyaNLP.Helper;
using XiaoyaStore.Data.Model;

namespace XiaoyaFileParser.Parsers
{
    public abstract class BaseParser : IFileParser
    {
        protected UrlFile mUrlFile;
        public UrlFile UrlFile
        {
            get => mUrlFile;
            set
            {
                mUrlFile = value;
                mContent = null;
                mLinkInfo = null;
                mHeaders = null;
                mTitle = TextHelper.NormalizeString(mUrlFile.Title);
                mTextContent = TextHelper.NormalizeString(mUrlFile.Content);
                mPublishDate = mUrlFile.PublishDate;
            }
        }

        public string FilePath { get; set; }

        protected FileParserConfig mConfig = new FileParserConfig();
        protected string mTitle = null;
        protected string mContent = null;
        protected string mTextContent = null;
        protected List<LinkInfo> mLinkInfo = null;
        protected List<string> mHeaders = null;
        protected DateTime mPublishDate = DateTime.MinValue;

        public BaseParser() { }

        public BaseParser(FileParserConfig config)
        {
            mConfig = config;
        }

        public virtual async Task<IList<Token>> GetTokensAsync()
        {
            var textContent = await GetTextContentAsync();
            textContent = TextHelper.RemoveConsecutiveNonsense(textContent);

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
                    Length = segment.Key.Length,
                    WordFrequency = segment.Count(),
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
                    wordDict[segment.Key].OccurencesInTitle = segment.Count();
                    wordDict[segment.Key].WordFrequency += segment.Count();
                }
                else
                {
                    var token = new Token
                    {
                        Word = segment.Key,
                        Positions = new List<int>(),
                        Length = segment.Key.Length,
                        WordFrequency = segment.Count(),
                        OccurencesInLinks = 0,
                        OccurencesInTitle = segment.Count(),
                        OccurencesInHeaders = 0,
                    };
                    result.Add(token);
                    wordDict.Add(token.Word, token);
                }
            }

            foreach (var header in headers)
            {
                foreach (var segment in mConfig.TextSegmenter.Segment(header).GroupBy(o => TextHelper.NormalizeIndexWord(o.Word)))
                {
                    if (wordDict.ContainsKey(segment.Key))
                    {
                        wordDict[segment.Key].OccurencesInHeaders = segment.Count();
                    }
                    else
                    {
                        var token = new Token
                        {
                            Word = segment.Key,
                            Positions = new List<int>(),
                            Length = segment.Key.Length,
                            WordFrequency = segment.Count(),
                            OccurencesInLinks = 0,
                            OccurencesInTitle = 0,
                            OccurencesInHeaders = segment.Count(),
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
                        wordDict[segment.Key].OccurencesInLinks += segment.Count();
                        wordDict[segment.Key].WordFrequency += segment.Count();
                    }
                    else
                    {
                        var token = new Token
                        {
                            Word = segment.Key,
                            Positions = new List<int>(),
                            Length = segment.Key.Length,
                            WordFrequency = segment.Count(),
                            OccurencesInLinks = segment.Count(),
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
                var content = await GetContentAsync();

                await Task.Run(() =>
                {
                    mTitle = content.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    if (mTitle == null)
                    {
                        mTitle = "";
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

        public virtual async Task<IList<string>> GetHeadersAsync()
        {
            return await Task.Run(() =>
            {
                return new List<string>();
            });
        }
    }
}
