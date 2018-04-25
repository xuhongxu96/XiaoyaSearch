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
    public class PdfFileParser : BaseParser, IFileParser
    {
#if DEBUG
        protected const string exeFileName = "../../../../Resources/pdftotext.exe";
#else
        protected const string exeFileName = "../Resources/pdftotext.exe";
#endif

        public PdfFileParser() : base() { }

        public PdfFileParser(FileParserConfig config) : base(config)
        { }

        public override async Task<string> GetContentAsync()
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
    }
}
