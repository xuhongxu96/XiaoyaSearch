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

namespace XiaoyaFileParser.Parsers
{
    public class TextFileParser : IFileParser
    {
        protected string mEncoding;

        protected UrlFile mUrlFile;
        public UrlFile UrlFile
        {
            get => mUrlFile;
            set
            {
                mUrlFile = value;
                mContent = null;
                mLinkInfo = null;
                mTitle = mUrlFile.Title;
                mTextContent = mUrlFile.Content;
                mEncoding = mUrlFile.Charset;
                if (new FileInfo(mUrlFile.FilePath).Length > 4 * 1024 * 1024)
                {
                    throw new FileLoadException(mUrlFile.FilePath + " is too big to parse");
                }
            }
        }

        protected FileParserConfig mConfig = new FileParserConfig();
        protected string mTitle = null;
        protected string mContent = null;
        protected string mTextContent = null;
        protected List<LinkInfo> mLinkInfo = null;

        public TextFileParser() { }

        public TextFileParser(FileParserConfig config)
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

            foreach (var segment in mConfig.TextSegmenter.Segment(textContent))
            {
                result.Add(new Token
                {
                    Text = segment.Text.ToLower(),
                    Position = segment.Position,
                    Length = segment.Length,
                    Type = TokenType.Body,
                });
            }

            foreach (var segment in mConfig.TextSegmenter.Segment(title))
            {
                result.Add(new Token
                {
                    Text = segment.Text.ToLower(),
                    Position = segment.Position,
                    Length = segment.Length,
                    Type = TokenType.Title,
                });
            }

            return result;
        }

        public virtual async Task<IList<Token>> GetTokensAsync(IEnumerable<LinkInfo> linkInfos)
        {
            var result = await GetTokensAsync();

            var positionOffset = 0;
            foreach (var link in linkInfos.Distinct())
            {
                foreach (var segment in mConfig.TextSegmenter.Segment(link.Text))
                {
                    result.Add(new Token
                    {
                        Text = segment.Text.ToLower(),
                        Position = positionOffset + segment.Position,
                        Length = segment.Length,
                        Type = TokenType.Link,
                    });
                }
                positionOffset += link.Text.Length;
            }

            return result;
        }

        public async Task<string> GetContentAsync()
        {
            if (mContent == null)
            {
                if (mEncoding == null)
                {
                    mEncoding = EncodingDetector.GetEncoding(UrlFile.FilePath);
                    if (mEncoding == null)
                    {
                        throw new NotSupportedException($"Invalid text encoding: {UrlFile.Url}");
                    }
                }
                mContent = await File.ReadAllTextAsync(UrlFile.FilePath,
                    Encoding.GetEncoding(mEncoding));
                mContent = TextHelper.FullWidthCharToHalfWidthChar(mContent.ToLower());
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
    }
}
