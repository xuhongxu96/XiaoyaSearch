using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XiaoyaFileParser.Model;
using XiaoyaFileParser.Config;
using XiaoyaNLP.TextSegmentation;
using XiaoyaStore.Data.Model;

namespace XiaoyaFileParser
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
                mTextContent = mUrlFile.Content;
            }
        }

        protected FileParserConfig mConfig = new FileParserConfig();
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

            var result = new List<Token>();

            foreach (var segment in mConfig.TextSegmenter.Segment(textContent))
            {
                result.Add(new Token
                {
                    Text = segment.Text,
                    Position = segment.Position,
                    Length = segment.Length,
                });
            }

            return result;
        }

        public async Task<string> GetContentAsync()
        {
            if (mContent == null)
            {
                mContent = await File.ReadAllTextAsync(UrlFile.FilePath);
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
    }
}
