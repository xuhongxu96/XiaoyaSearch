using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace XiaoyaNLP.Helper
{
    public static class TextHelper
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

        public static IEnumerable<DateTime> ExtractDateTime(string input)
        {
            var match = CommonRegex.DateRegex.Match(input);
            while (match.Success)
            {
                DateTime result;

                var group = match.Groups;
                if (group.Count == 4
                    && int.TryParse(group[1].Value, out int year)
                    && int.TryParse(group[2].Value, out int month)
                    && int.TryParse(group[3].Value, out int day))
                {
                    try
                    {
                        result = new DateTime(year, month, day);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        match.NextMatch();
                        continue;
                    }
                    yield return result;
                }

                match.NextMatch();
            }
        }
    }
}
