using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaFileParser.Model;
using XiaoyaFileParser.Config;
using XiaoyaStore.Data.Model;
using System.IO;
using XiaoyaFileParser.Parsers;
using XiaoyaNLP.Encoding;

namespace XiaoyaFileParser
{
    public class UniversalFileParser : IFileParser
    {
        protected UrlFile mUrlFile;
        public UrlFile UrlFile
        {
            get => mUrlFile;
            set
            {
                mUrlFile = value;
                if (!File.Exists(mUrlFile.FilePath))
                {
                    throw new FileNotFoundException(mUrlFile.FilePath + " doesn't exist");
                }
                mCurrentParser = GetParser(mUrlFile.MimeType, mConfig);
                mCurrentParser.UrlFile = mUrlFile;
            }
        }

        protected FileParserConfig mConfig = new FileParserConfig();
        protected IFileParser mCurrentParser = null;

        public UniversalFileParser() { }

        public UniversalFileParser(FileParserConfig config)
        {
            mConfig = config;
        }

        public async Task<string> GetTitleAsync()
        {
            return await mCurrentParser?.GetTitleAsync();
        }

        public async Task<string> GetContentAsync()
        {
            return await mCurrentParser?.GetContentAsync();
        }

        public async Task<string> GetTextContentAsync()
        {
            return await mCurrentParser?.GetTextContentAsync();
        }

        public async Task<IList<Token>> GetTokensAsync()
        {
            return await mCurrentParser?.GetTokensAsync();
        }

        public async Task<IList<string>> GetUrlsAsync()
        {
            return await mCurrentParser?.GetUrlsAsync();
        }

        public static bool IsSupported(string mimeType)
        {
            switch (mimeType)
            {
                case "text/html":
                case "text/plain":
                    return true;
                default:
                    return false;
            }
        }

        public static IFileParser GetParser(string mimeType, FileParserConfig config)
        {
            switch (mimeType)
            {
                case "text/html":
                    return new HtmlFileParser(config);
                case "text/plain":
                    return new TextFileParser(config);
                default:
                    throw new NotSupportedException(mimeType + " not supported");
            }
        }
    }
}
