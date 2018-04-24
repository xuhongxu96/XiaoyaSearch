using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XiaoyaFileParser.Model;
using XiaoyaFileParser.Config;
using XiaoyaNLP.TextSegmentation;
using XiaoyaStore.Data.Model;
using System.Linq;
using static XiaoyaFileParser.Model.Token;
using XiaoyaNLP.Encoding;
using XiaoyaNLP.Helper;
using System.Diagnostics;

namespace XiaoyaFileParser.Parsers
{
    public class PdfFileParser : IFileParser
    {
#if DEBUG
        protected const string exeFileName = "../../../../Resources/pdftotext.exe";
#else
        protected const string exeFileName = "../Resources/pdftotext.exe";
#endif

        protected UrlFile mUrlFile;
        public UrlFile UrlFile
        {
            get => mUrlFile;
            set
            {
                mUrlFile = value;
                mContent = null;
                mLinkInfo = null;
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
        protected DateTime mPublishDate = DateTime.MinValue;

        public PdfFileParser() { }

        public PdfFileParser(FileParserConfig config)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            mConfig = config;
        }

        public virtual async Task<IList<Token>> GetTokensAsync()
        {
            var textContent = await GetTextContentAsync();
            textContent = TextHelper.RemoveConsecutiveNonsense(textContent);

            var title = await GetTitleAsync();

            var result = new List<Token>();
            var wordDict = new Dictionary<string, Token>();

            foreach (var segment in mConfig.TextSegmenter.Segment(textContent).GroupBy(o => o.Word))
            {
                var token = new Token
                {
                    Word = segment.Key,
                    Positions = segment.Select(o => o.Position).OrderBy(o => o).ToList(),
                    Length = segment.Key.Length,
                    WordFrequency = segment.Count(),
                    OccurenceInLinks = 0,
                    OccurenceInTitle = 0,
                };
                result.Add(token);
                wordDict.Add(token.Word, token);
            }

            foreach (var segment in mConfig.TextSegmenter.Segment(title).GroupBy(o => o.Word))
            {
                if (wordDict.ContainsKey(segment.Key))
                {
                    wordDict[segment.Key].OccurenceInTitle = segment.Count();
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
                        OccurenceInLinks = 0,
                        OccurenceInTitle = segment.Count(),
                    };
                    result.Add(token);
                    wordDict.Add(token.Word, token);
                }
            }

            return result;
        }

        public virtual async Task<IList<Token>> GetTokensAsync(IEnumerable<LinkInfo> linkInfos)
        {
            var result = await GetTokensAsync();
            var wordDict = result.ToDictionary(o => o.Word);

            foreach (var link in linkInfos.Distinct())
            {
                foreach (var segment in mConfig.TextSegmenter.Segment(link.Text).GroupBy(o => o.Word))
                {
                    if (wordDict.ContainsKey(segment.Key))
                    {
                        wordDict[segment.Key].OccurenceInLinks += segment.Count();
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
                            OccurenceInLinks = segment.Count(),
                            OccurenceInTitle = 0,
                        };
                        result.Add(token);
                        wordDict.Add(token.Word, token);
                    }
                }
            }

            return result;
        }

        public async Task<string> GetContentAsync()
        {
            if (mContent == null)
            {
                var tempOutput = Path.GetTempFileName();

                var process = Process.Start(exeFileName, "-enc UTF-8 " + FilePath + " " + tempOutput);
                process.WaitForExit();

                mContent = await File.ReadAllTextAsync(tempOutput);
                mContent = TextHelper.NormalizeString(mContent);
            }
            return mContent;
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
    }
}
