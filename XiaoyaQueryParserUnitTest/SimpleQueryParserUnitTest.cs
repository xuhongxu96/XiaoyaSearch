using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using XiaoyaQueryParser.Config;
using XiaoyaQueryParser.QueryParser;
using XiaoyaRetriever.Expression;

namespace XiaoyaQueryParserUnitTest
{
    [TestClass]
    public class SimpleQueryParserUnitTest
    {
        [TestMethod]
        public void TestParse()
        {
            var parser = new SimpleQueryParser(new QueryParserConfig());

            var expression = parser.Parse("Hello World -ÄãºÃ°¡");

            Assert.IsInstanceOfType(expression, typeof(And));

            var andExp = expression as And;
            var andExpList = andExp.ToList();

            Assert.IsInstanceOfType(andExpList[0], typeof(Word));
            Assert.IsInstanceOfType(andExpList[1], typeof(Word));
            Assert.IsInstanceOfType(andExpList[2], typeof(Not));

            Assert.AreEqual("hello", (andExpList[0] as Word).Value);
            Assert.AreEqual("world", (andExpList[1] as Word).Value);

            Assert.IsInstanceOfType((andExpList[2] as Not).Operand, typeof(And));

            var notAndExp = (andExpList[2] as Not).Operand as And;
            var notWord1 = notAndExp.ToList()[0];
            var notWord2 = notAndExp.ToList()[1];

            Assert.IsInstanceOfType(notWord1, typeof(Word));
            Assert.IsInstanceOfType(notWord2, typeof(Word));

            Assert.AreEqual("ÄãºÃ", (notWord1 as Word).Value);
            Assert.AreEqual("°¡", (notWord2 as Word).Value);
        }

    }
}
