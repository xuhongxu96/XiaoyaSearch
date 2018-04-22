using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaNLP.TextSegmentation;

namespace XiaoyaNLPUnitTest.TextSegmentation
{
    [TestClass]
    public class NGramUnitTest
    {
        [TestMethod]
        public void Test2Gram()
        {
            var nGram = new NGram();
            var segments = nGram.Segment("大家好！Hello world我是大笨蛋，哈哈哈哈2018!");
            var words = segments.Select(o => o.Word);

            Assert.IsTrue(words.Contains("家好"));
            Assert.IsTrue(words.Contains("lo"));
            Assert.IsTrue(words.Contains("笨蛋"));
            Assert.IsTrue(words.Contains("18"));

            Assert.IsFalse(words.Contains("ow"));
            Assert.IsFalse(words.Contains("好h"));
            Assert.IsFalse(words.Contains("蛋哈"));
            Assert.IsFalse(words.Contains("哈2"));

            foreach (var segment in segments)
            {
                Console.WriteLine(string.Format("{0}: {1}", segment.Word, segment.Position));
            }
        }

        [TestMethod]
        public void Test3Gram()
        {
            var nGram = new NGram(3);
            var segments = nGram.Segment("大家好！Hello world我是大笨蛋，哈哈哈哈2018!");
            var words = segments.Select(o => o.Word);

            Assert.IsTrue(words.Contains("大家好"));
            Assert.IsTrue(words.Contains("llo"));
            Assert.IsTrue(words.Contains("大笨蛋"));
            Assert.IsTrue(words.Contains("201"));

            Assert.IsFalse(words.Contains("low"));
            Assert.IsFalse(words.Contains("家好h"));
            Assert.IsFalse(words.Contains("蛋哈哈"));
            Assert.IsFalse(words.Contains("哈20"));

            foreach (var segment in segments)
            {
                Console.WriteLine(string.Format("{0}: {1}", segment.Word, segment.Position));
            }
        }

        [TestMethod]
        public void Test2GramSegmentOnlyChinese()
        {
            var nGram = new NGram(2, true);
            var segments = nGram.Segment("大家好！Hello world我是大笨蛋，哈哈哈哈2018!");
            var words = segments.Select(o => o.Word);

            Assert.IsTrue(words.Contains("家好"));
            Assert.IsTrue(words.Contains("Hello"));
            Assert.IsTrue(words.Contains("world"));
            Assert.IsTrue(words.Contains("笨蛋"));
            Assert.IsTrue(words.Contains("2018"));

            Assert.IsFalse(words.Contains("lo"));
            Assert.IsFalse(words.Contains("wo"));
            Assert.IsFalse(words.Contains("20"));
            Assert.IsFalse(words.Contains("18"));

            foreach (var segment in segments)
            {
                Console.WriteLine(string.Format("{0}: {1}", segment.Word, segment.Position));
            }
        }
    }
}
