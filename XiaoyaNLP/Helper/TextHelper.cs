using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaNLP.Helper
{
    public class TextHelper
    {
        public static string ReplaceSpaces(string input, string replacement = "\n")
        {
            return CommonRegex.Trimmer.Replace(input.Trim(), replacement);
        }

        public static string FullWidthCharToHalfWidthChar(string input)
        {
            return input.Normalize(NormalizationForm.FormKC);
        }
    }
}
