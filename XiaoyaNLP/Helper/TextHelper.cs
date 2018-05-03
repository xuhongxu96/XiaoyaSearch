using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using XiaoyaNLP.WordStemmer;

namespace XiaoyaNLP.Helper
{
    public static class TextHelper
    {
        public static string ReplaceSpaces(string input, string replacement = "\n")
        {
            return CommonRegex.ConsecutiveTrimmer.Replace(input.Trim(), replacement);
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

        public static string RemoveSpecialCharacters(string input)
        {
            return CommonRegex.AllNonChars.Replace(input, "");
        }

        public static string NormalizeString(string input)
        {
            if (input == null) return null;

            var result = FullWidthCharToHalfWidthChar(input.Trim());
            result = RemoveSpecialCharacters(result);
            result = result.ToLower();

            return result;
        }

        public static string NormalizeIndexWord(string input)
        {
            if (input == null) return null;

            var result = FullWidthCharToHalfWidthChar(input);
            result = RemoveSpecialCharacters(result);
            result = new EnglishPorter2Stemmer().Stem(result).Value;
            result = result.ToLower();

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
                        match = match.NextMatch();
                        continue;
                    }
                    yield return result;
                }

                match = match.NextMatch();
            }
        }
    }
}
