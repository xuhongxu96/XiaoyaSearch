using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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

        public static string RemoveConsecutiveNonsense(string input)
        {
            var result = CommonRegex.ConsecutiveSymbolNumbers.Replace(input, new MatchEvaluator(match =>
            {
                if (match.Value.Length > 12)
                {
                    return "";
                }
                return match.Value;
            }));

            result = CommonRegex.ConsecutiveWeekDay.Replace(result, new MatchEvaluator(match =>
            {
                var weekDayMatches = CommonRegex.WeekDay.Matches(match.Value);
                if (weekDayMatches.Count >= 7)
                {
                    return "";
                }
                return match.Value;
            }));

            return result;
        }
    }
}
