﻿using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaNLP.Helper;

namespace XiaoyaNLP.Encoding
{
    public static class EncodingDetector
    {
        public static bool IsValidString(string content)
        {
            var charCount = CommonRegex.RegexAllFullWidthChar.Matches(content).Count;
            var notCharCount = CommonRegex.RegexAllNotChar.Matches(content).Count;

            if (notCharCount >= Math.Max(charCount / 2, 5))
            {
                return false;
            }
            return true;
        }
    }
}