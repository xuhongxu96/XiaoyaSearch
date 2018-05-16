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
        public TextFileParser() : base() { }

        public TextFileParser(FileParserConfig config) : base(config)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public override async Task<string> GetContentAsync()
        {
            if (mContent == null)
            {
                mContent = await File.ReadAllTextAsync(mFilePath,
                    Encoding.GetEncoding(mCharset));
                mContent = TextHelper.NormalizeString(mContent);
            }
            return mContent;
        }
    }
}
