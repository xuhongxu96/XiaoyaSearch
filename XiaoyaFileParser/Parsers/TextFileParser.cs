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
using XiaoyaCommon.Helper;
using XiaoyaNLP.Encoding;

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
                mTitle = mUrlFile.Title;
                mTextContent = mUrlFile.Content;
                if (new FileInfo(mUrlFile.FilePath).Length > 4 * 1024 * 1024)
                {
                    throw new FileLoadException(mUrlFile.FilePath + " is too big to parse");
                }

                mEncoding = UrlFile.Charset;
                if (mEncoding == null || mEncoding == "")
                {
                    mEncoding = "utf-8";
                }
                mEncoding = mEncoding.ToLower();
            }
        }

        protected FileParserConfig mConfig = new FileParserConfig();
        protected string mTitle = null;
        protected string mContent = null;
        protected string mTextContent = null;

        public TextFileParser() { }

        public TextFileParser(FileParserConfig config)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            mConfig = config;
        }

        public virtual async Task<IList<Token>> GetTokensAsync()
        {
            var textContent = await GetTextContentAsync();
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

        public async Task<string> GetContentAsync()
        {
            if (mContent == null)
            {
                mContent = await File.ReadAllTextAsync(UrlFile.FilePath,
                    Encoding.GetEncoding(mEncoding));

                if (mEncoding == "utf-8" && !EncodingDetector.IsValidString(mContent))
                {
                    mEncoding = "gbk";
                    mContent = await File.ReadAllTextAsync(UrlFile.FilePath,
                        Encoding.GetEncoding(mEncoding));
                }

                mContent = TextHelper.ToDBC(mContent.ToLower());
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

        public virtual async Task<string> GetTitleAsync()
        {
            if (mTitle == null)
            {
                await Task.Run(() =>
                {
                    mTitle = mContent.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
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
