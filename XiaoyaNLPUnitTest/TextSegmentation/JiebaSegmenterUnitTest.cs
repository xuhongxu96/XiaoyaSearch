using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XiaoyaNLP.TextSegmentation;

namespace XiaoyaNLPUnitTest.TextSegmentation
{
    [TestClass]
    public class JiebaSegmenterUnitTest
    {
        [TestMethod]
        public void TestJiebaSegmenter()
        {
            var segmenter = new JiebaSegmenter();
            var segments = segmenter.Segment("1.2中文分词中华人民共和国Hello World!你\n好，我 们2.2.3.4");
            var words = segments.Select(o => o.Word);

            Assert.IsTrue(words.Contains("1.2"));
            Assert.IsTrue(words.Contains("中文"));
            Assert.IsTrue(words.Contains("分词"));
            Assert.IsTrue(words.Contains("中华人民共和国"));
            Assert.IsTrue(words.Contains("你"));
            Assert.IsTrue(words.Contains("好"));
            Assert.IsTrue(words.Contains("我"));
            Assert.IsTrue(words.Contains("们"));
            Assert.IsTrue(words.Contains("Hello"));
            Assert.IsTrue(words.Contains("World"));
            Assert.IsTrue(words.Contains("2.2"));
            Assert.IsTrue(words.Contains("3.4"));

            foreach (var segment in segments)
            {
                Console.WriteLine(string.Format("{0}: {1}", segment.Word, segment.Position));
            }
        }

        [TestMethod]
        public void TestJiebaSegmenterMultiThread()
        {
            var segmenter = new JiebaSegmenter();
            var tasks = new List<Task>();

            for (int i = 0; i < 1000; ++i)
            {
                tasks.Add(Task.Run(() =>
                {
                    var segments = segmenter.Segment("1.2中文分词中华人民共和国Hello World!你\n好，我 们2.2.3.4");
                    var words = segments.Select(o => o.Word);

                    Assert.IsTrue(words.Contains("1.2"));
                    Assert.IsTrue(words.Contains("中文"));
                    Assert.IsTrue(words.Contains("分词"));
                    Assert.IsTrue(words.Contains("中华人民共和国"));
                    Assert.IsTrue(words.Contains("你"));
                    Assert.IsTrue(words.Contains("好"));
                    Assert.IsTrue(words.Contains("我"));
                    Assert.IsTrue(words.Contains("们"));
                    Assert.IsTrue(words.Contains("Hello"));
                    Assert.IsTrue(words.Contains("World"));
                    Assert.IsTrue(words.Contains("2.2"));
                    Assert.IsTrue(words.Contains("3.4"));

                    foreach (var segment in segments)
                    {
                        Console.WriteLine(string.Format("{0}: {1}", segment.Word, segment.Position));
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
