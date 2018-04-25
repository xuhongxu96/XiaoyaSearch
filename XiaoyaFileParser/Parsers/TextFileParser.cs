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
    public class TextFileParser : BaseParser, IFileParser
    {
        protected string mEncoding;

        public TextFileParser() : base() { }

        public TextFileParser(FileParserConfig config) : base(config)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public override async Task<string> GetContentAsync()
        {
            if (mContent == null)
            {
                if (mEncoding == null)
                {
                    mEncoding = EncodingDetector.GetEncoding(FilePath);
                    if (mEncoding == null)
                    {
                        throw new NotSupportedException($"Invalid text encoding: {UrlFile.Url}");
                    }
                }
                mContent = await File.ReadAllTextAsync(FilePath,
                    Encoding.GetEncoding(mEncoding));
                mContent = TextHelper.NormalizeString(mContent);
            }
            return mContent;
        }
    }
}
