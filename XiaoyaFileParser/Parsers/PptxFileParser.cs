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
using DocumentFormat.OpenXml.Presentation;

namespace XiaoyaFileParser.Parsers
{
    public class PptxFileParser : OfficeParser, IFileParser
    {
        public PptxFileParser() : base() { }

        public PptxFileParser(FileParserConfig config) : base(config)
        { }

        // Count the slides in the presentation.
        private static int CountSlides(PresentationDocument presentationDocument)
        {
            // Check for a null document object.
            if (presentationDocument == null)
            {
                throw new ArgumentNullException("presentationDocument");
            }

            int slidesCount = 0;

            // Get the presentation part of document.
            PresentationPart presentationPart = presentationDocument.PresentationPart;
            // Get the slide count from the SlideParts.
            if (presentationPart != null)
            {
                slidesCount = presentationPart.SlideParts.Count();
            }
            // Return the slide count to the previous method.
            return slidesCount;
        }

        private static void GetSlideIdAndText(out string sldText, PresentationDocument ppt, int index)
        {
            // Get the relationship ID of the first slide.
            PresentationPart part = ppt.PresentationPart;
            OpenXmlElementList slideIds = part.Presentation.SlideIdList.ChildElements;

            string relId = (slideIds[index] as SlideId).RelationshipId;

            // Get the slide part from the relationship ID.
            SlidePart slide = (SlidePart)part.GetPartById(relId);

            sldText = slide.Slide.InnerText;
        }

        // Get a list of the titles of all the slides in the presentation.
        private static IList<string> GetSlideTitles(PresentationDocument presentationDocument)
        {
            if (presentationDocument == null)
            {
                throw new ArgumentNullException("presentationDocument");
            }

            // Get a PresentationPart object from the PresentationDocument object.
            PresentationPart presentationPart = presentationDocument.PresentationPart;

            if (presentationPart != null &&
                presentationPart.Presentation != null)
            {
                // Get a Presentation object from the PresentationPart object.
                Presentation presentation = presentationPart.Presentation;

                if (presentation.SlideIdList != null)
                {
                    List<string> titlesList = new List<string>();

                    // Get the title of each slide in the slide order.
                    foreach (var slideId in presentation.SlideIdList.Elements<SlideId>())
                    {
                        SlidePart slidePart = presentationPart.GetPartById(slideId.RelationshipId) as SlidePart;

                        // Get the slide title.
                        string title = TextHelper.NormalizeString(GetSlideTitle(slidePart));

                        if (title != "")
                        {
                            titlesList.Add(title);
                        }
                    }

                    return titlesList;
                }

            }

            return null;
        }

        // Determines whether the shape is a title shape.
        private static bool IsTitleShape(Shape shape)
        {
            var placeholderShape = shape.NonVisualShapeProperties.ApplicationNonVisualDrawingProperties.GetFirstChild<PlaceholderShape>();
            if (placeholderShape != null && placeholderShape.Type != null && placeholderShape.Type.HasValue)
            {
                switch ((PlaceholderValues)placeholderShape.Type)
                {
                    // Any title shape.
                    case PlaceholderValues.Title:

                    // A centered title.
                    case PlaceholderValues.CenteredTitle:
                        return true;

                    default:
                        return false;
                }
            }
            return false;
        }

        // Get the title string of the slide.
        private static string GetSlideTitle(SlidePart slidePart)
        {
            if (slidePart == null)
            {
                throw new ArgumentNullException("presentationDocument");
            }

            if (slidePart.Slide != null)
            {
                // Find all the title shapes.
                var shapes = from shape in slidePart.Slide.Descendants<Shape>()
                             where IsTitleShape(shape)
                             select shape;

                StringBuilder paragraphText = new StringBuilder();

                foreach (var shape in shapes)
                {
                    // Get the text in each paragraph in this shape.
                    paragraphText.AppendLine(shape.InnerText);
                }

                return paragraphText.ToString();
            }

            return string.Empty;
        }

        public override async Task<string> GetContentAsync()
        {
            if (mContent == null)
            {
                await Task.Run(() =>
                {
                    var textBuilder = new StringBuilder();
                    using (var doc = PresentationDocument.Open(mUrlFile.FilePath, false))
                    {
                        if (doc == null)
                        {
                            throw new ArgumentNullException("Invalid PPTX");
                        }

                        var numberOfSlides = CountSlides(doc);

                        for (int i = 0; i < numberOfSlides; i++)
                        {
                            GetSlideIdAndText(out string slideText, doc, i);
                            textBuilder.AppendLine(slideText);
                        }

                        mHeaders = GetSlideTitles(doc).ToList();
                    }
                    mContent = textBuilder.ToString();
                });
                mContent = TextHelper.NormalizeString(mContent);
                mContent = mContent.Length + "\n" + mContent + string.Join("\n", mHeaders);
            }
            return mContent;
        }
    }
}
