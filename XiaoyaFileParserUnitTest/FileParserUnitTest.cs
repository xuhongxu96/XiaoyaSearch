using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using XiaoyaCommon.Helper;
using XiaoyaFileParser.Parsers;

namespace XiaoyaFileParserUnitTest
{
    [TestClass]
    public class FileParserUnitTest
    {
        [TestMethod]
        public void TestMimeDetect()
        {
            var mime = MimeHelper.GetContentType("a.docx");
            Assert.AreEqual("application/vnd.openxmlformats-officedocument.wordprocessingml.document", mime);
            mime = MimeHelper.GetContentType("b.pptx");
            Assert.AreEqual("application/vnd.openxmlformats-officedocument.presentationml.presentation", mime);
        }

        [TestMethod]
        public void TestDocxParser()
        {
            var parser = new DocxFileParser
            {
                UrlFile = new XiaoyaStore.Data.Model.UrlFile
                {
                    FilePath = "a.docx",
                }
            };
            var headers = parser.GetHeadersAsync().GetAwaiter().GetResult();
            foreach (var header in headers)
            {
                Console.WriteLine(header);
            }
            Assert.IsTrue(headers.Select(o => o.Text).Contains("可行性论述"));

            var content = parser.GetContentAsync().GetAwaiter().GetResult();
            Console.WriteLine(content);
            Assert.IsTrue(content.Contains("从基础架构开始"));
            Assert.IsTrue(content.Contains("并在论文中分模块进行撰写"));
        }

        [TestMethod]
        public void TestPptxParser()
        {
            var parser = new PptxFileParser
            {
                UrlFile = new XiaoyaStore.Data.Model.UrlFile
                {
                    FilePath = "b.pptx",
                }
            };
            var headers = parser.GetHeadersAsync().GetAwaiter().GetResult();
            foreach (var header in headers)
            {
                Console.WriteLine(header);
            }
            Assert.IsTrue(headers.Select(o => o.Text).Contains("应用领域"));

            var content = parser.GetContentAsync().GetAwaiter().GetResult();
            Console.WriteLine(content);
            Assert.IsTrue(content.Contains("研究关键领域集中于雷达"));
        }
    }
}
