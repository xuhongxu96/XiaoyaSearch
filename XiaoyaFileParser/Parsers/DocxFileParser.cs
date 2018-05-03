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
using DocumentFormat.OpenXml;

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
                    var headers = new List<(string Text, int FontSize)>();
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

                            OpenXmlElement currentPar = null;

                            foreach (var run in body.Descendants<Run>())
                            {
                                if (run == null)
                                {
                                    continue;
                                }

                                var props = run.RunProperties;
                                if (props != null)
                                {
                                    if (props.FontSize == null
                                        || props.FontSize.Val == null
                                        || !int.TryParse(props.FontSize.Val.Value, out int fontSize))
                                    {
                                        if (props.FontSizeComplexScript == null
                                        || props.FontSizeComplexScript.Val == null
                                        || !int.TryParse(props.FontSizeComplexScript.Val.Value, out fontSize))
                                        {
                                            fontSize = 24;
                                        }
                                    }

                                    if (props.Bold == null && fontSize <= 36)
                                    {
                                        // Not considered as header
                                        continue;
                                    }

                                    if (currentPar == run.Parent)
                                    {
                                        continue;
                                    }
                                    currentPar = run.Parent;
                                    var text = TextHelper.NormalizeString(currentPar.InnerText.Trim());
                                    if (text != "" && TextHelper.ReplaceSpaces(text, "").Length < 50)
                                    {
                                        headers.Add((text, fontSize));
                                    }
                                }
                            }
                        }
                    }
                    mContent = TextHelper.NormalizeString(textBuilder.ToString());
                    var maxFontSize = headers.Max(o => o.FontSize);

                    var firstLine = (mContent.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault() ?? "").Trim();

                    var title = string.Join(" ", headers
                        .Where(o => o.FontSize == maxFontSize || o.FontSize > 36)
                        .OrderByDescending(o => o.FontSize)
                        .Take(3)
                        .Select(o => o.Text));

                    if (!title.Contains(firstLine))
                    {
                        title = firstLine + " " + title;
                    }

                    headers.Insert(0, (title, int.MaxValue));

                    mContent = mContent.Length + "\n" + mContent
                        + string.Join("\n", headers.Select(o => o.Text));
                });

            }
            return mContent;
        }


    }
}
