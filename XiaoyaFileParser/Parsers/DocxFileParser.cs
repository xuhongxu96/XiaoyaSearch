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
    public class DocxFileParser : OfficeParser, IFileParser
    {
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

                            foreach (var run in body.Descendants<Run>())
                            {
                                if (run == null)
                                {
                                    continue;
                                }

                                var props = run.RunProperties;
                                if (props != null && props.Bold != null)
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
                    mContent = textBuilder.ToString();
                    mContent = mContent.Length + "\n" + mContent + string.Join("\n", mHeaders);
                });
                mContent = TextHelper.NormalizeString(mContent);
            }
            return mContent;
        }

        
    }
}
