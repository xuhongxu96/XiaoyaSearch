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

namespace XiaoyaFileParser.Parsers
{
    public class TextFileParser : IFileParser
    {
        protected UrlFile mUrlFile;
        public UrlFile UrlFile
        {
            get => mUrlFile;
            set
            {
                mUrlFile = value;
                mTitle = null;
                mContent = null;
                mTextContent = mUrlFile.Content;
            }
        }

        protected FileParserConfig mConfig = new FileParserConfig();
        protected string mTitle = null;
        protected string mContent = null;
        protected string mTextContent = null;

        public TextFileParser() { }

        public TextFileParser(FileParserConfig config)
        {
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
                    Encoding.GetEncoding(UrlFile.Charset));
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
            return await Task.Run(() =>
            {
                if (mTitle == null)
                {
                    mTitle = File.ReadLines(UrlFile.FilePath).FirstOrDefault();
                }
                return mTitle;
            });
        }
    }
}
