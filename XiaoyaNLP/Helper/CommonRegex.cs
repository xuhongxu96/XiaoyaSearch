using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace XiaoyaNLP.Helper
{
    public static class CommonRegex
    {
        public static readonly Regex ConsecutiveTrimmer = new Regex(@"\s\s+|　", RegexOptions.Compiled );

        public static readonly Regex AllChars = new Regex(@"([\u4E00-\u9FA0\x00-\xff]+)", RegexOptions.Compiled);
        public static readonly Regex AllNonChars = new Regex(@"([^\u4E00-\u9FA0\x00-\xff]+)", RegexOptions.Compiled);
        public static readonly Regex AnyChars = new Regex(@"([\u4E00-\u9FD5]+|[a-zA-Z]+|\d{1,5}\.\d{1,5}|\d{1,8}|\d{11})", RegexOptions.Compiled);

        public static readonly Regex ConsecutiveSymbolNumbers = new Regex(@"(\d[一二三四五六七八九十\d\s-+/*!@#$%^&*()\.]+)", RegexOptions.Compiled);
        public static readonly Regex ConsecutiveWeekDay = new Regex(@"([一二三四五六日\d\s-+/*!@#$%^&*()\.]+)", RegexOptions.Compiled);

        public static readonly Regex WeekDay = new Regex(@"([一二三四五六日])", RegexOptions.Compiled);

        public static readonly Regex DateRegex = new Regex(@"[^\d](19\d\d|20\d\d)\s{0,3}[^\d]\s{0,3}(0[1-9]|1[0-2])\s{0,3}[^\d]\s{0,3}(0[1-9]|1\d|2\d|3[01])[^\d]", RegexOptions.Compiled);

        public static readonly Regex ChineseChars = new Regex(@"([\u4E00-\u9FA0]+)", RegexOptions.Compiled);
        public static readonly Regex EnglishChars = new Regex(@"[a-zA-Z]", RegexOptions.Compiled);
        public static readonly Regex DigitChars = new Regex(@"\n", RegexOptions.Compiled);

        public static readonly Regex UserDict = new Regex("^(?<word>.+?)(?<freq> [0-9]+)?(?<tag> [a-z]+)?$", RegexOptions.Compiled);

    }
}
