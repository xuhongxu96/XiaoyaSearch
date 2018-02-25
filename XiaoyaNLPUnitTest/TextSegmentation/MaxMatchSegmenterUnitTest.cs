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
            var segmenter = new MaxMatchSegmenter("Resource/30wdict_utf8.txt");
            var segments = segmenter.Segment("中文分词中华人民共和国");
            var words = segments.Select(o => o.Text);

            Assert.IsTrue(words.Contains("中文"));
            Assert.IsTrue(words.Contains("分词"));
            Assert.IsTrue(words.Contains("中华人民共和国"));

            foreach (var segment in segments)
            {
                Console.WriteLine(string.Format("{0}: {1}", segment.Text, segment.Position));
            }
        }
    }
}
