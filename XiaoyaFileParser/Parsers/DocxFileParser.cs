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
using DocumentFormat.OpenXml.Packaging;
using System.Xml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace XiaoyaFileParser.Parsers
{
    public class DocxFileParser : BaseParser, IFileParser
    {
        private const string WordmlNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        public DocxFileParser() : base() { }

        public DocxFileParser(FileParserConfig config) : base(config)
        { }

        public override async Task<string> GetContentAsync()
        {
            if (mContent == null)
            {
                await Task.Run(() =>
                {
                    var textBuilder = new StringBuilder();
                    using (var doc = WordprocessingDocument.Open(mUrlFile.FilePath, false))
                    {
                        if (doc == null)
                        {
                            throw new ArgumentNullException("Invalid DOCX");
                        }
                        Body body = doc.MainDocumentPart.Document.Body;
                        if (body != null)
                        {
                            foreach (var par in body.Descendants<Paragraph>())
                            {
                                textBuilder.AppendLine(par.InnerText);
                            }
                        }
                    }
                    mContent = textBuilder.ToString();
                });
                mContent = TextHelper.NormalizeString(mContent);
            }
            return mContent;
        }

        public override async Task<IList<string>> GetHeadersAsync()
        {
            if (mHeaders == null)
            {
                mHeaders = new List<string>();
                await Task.Run(() =>
                {
                    using (var doc = WordprocessingDocument.Open(mUrlFile.FilePath, false))
                    {
                        Body body = doc.MainDocumentPart.Document.Body;
                        if (body != null)
                        {
                            foreach (var run in body.Descendants<Run>())
                            {
                                var props = run.RunProperties;
                                if (props.Bold != null)
                                {
                                    var text = TextHelper.NormalizeString(run.InnerText.Trim());
                                    if (text != "")
                                    {
                                        mHeaders.Add(text);
                                    }
                                }
                            }
                        }
                    }
                });
            }
            return mHeaders;
        }
    }
}
