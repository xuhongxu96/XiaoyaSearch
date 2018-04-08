using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace XiaoyaNLP.Helper
{
    public static class CommonRegex
    {
        public static readonly Regex RegexAllFullWidthChar = new Regex(@"([【】\u2014\uFF0E\u3002\uff1b\uff0c\uff1a\u201c\u201d\uff08\uff09\u3001\uff1f\u300a\u300b\u4E00-\u9FD5])", RegexOptions.Compiled);
        public static readonly Regex RegexAllNotChar = new Regex(@"([^【】\u2014\uFF0E\u3002\uff1b\uff0c\uff1a\u201c\u201d\uff08\uff09\u3001\uff1f\u300a\u300b\x00-\xff\u4E00-\u9FD5\w\s])", RegexOptions.Compiled);
        public static readonly Regex RegexAllSymbol = new Regex(@"([.+#&_])", RegexOptions.Compiled);

        public static readonly Regex RegexAllChars = new Regex(@"([\u4E00-\u9FD5a-zA-Z0-9]+)", RegexOptions.Compiled);
        public static readonly Regex RegexAnyChars = new Regex(@"([\u4E00-\u9FD5]+|[a-zA-Z]+|\d{1,5}\.\d{1,5}|\d{1,8}|\d{11})", RegexOptions.Compiled);

        public static readonly Regex RegexChineseChars = new Regex(@"([\u4E00-\u9FD5]+)", RegexOptions.Compiled);
        public static readonly Regex RegexEnglishChars = new Regex(@"[a-zA-Z0-9]", RegexOptions.Compiled);

        public static readonly Regex RegexUserDict = new Regex("^(?<word>.+?)(?<freq> [0-9]+)?(?<tag> [a-z]+)?$", RegexOptions.Compiled);

    }
}
