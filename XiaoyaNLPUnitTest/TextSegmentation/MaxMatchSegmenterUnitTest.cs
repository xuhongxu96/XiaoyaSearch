using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaNLP.TextSegmentation;

namespace XiaoyaNLPUnitTest.TextSegmentation
{
    [TestClass]
    public class MaxMatchSegmenterUnitTest
    {
        [TestMethod]
        public void TestMaxMatchSegmenter()
        {
            var segmenter = new MaxMatchSegmenter("../../../../Resources/30wdict_utf8.txt");
            var segments = segmenter.Segment("中文分词中华人民共和国Hello World!你\n好，我 们");
            var words = segments.Select(o => o.Text);

            Assert.IsTrue(words.Contains("中文"));
            Assert.IsTrue(words.Contains("分词"));
            Assert.IsTrue(words.Contains("中华人民共和国"));
            Assert.IsTrue(words.Contains("你"));
            Assert.IsTrue(words.Contains("好"));
            Assert.IsTrue(words.Contains("我"));
            Assert.IsTrue(words.Contains("们"));
            Assert.IsTrue(words.Contains("Hello"));
            Assert.IsTrue(words.Contains("World"));

            foreach (var segment in segments)
            {
                Console.WriteLine(string.Format("{0}: {1}", segment.Text, segment.Position));
            }
        }
    }
}
