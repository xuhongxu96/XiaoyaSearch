using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Helper;

namespace XiaoyaCommon.Helper
{
    public static class ScoringHelper
    {
        public static double TfIdf(ulong frequency, ulong documentFrequency, ulong documentCount)
        {
            double idf = Math.Log(documentCount + 1) - Math.Log(documentFrequency + 1);
            return (1 + Math.Log(frequency + 0.1)) * idf;
        }

        public static double CalculateIndexWeight(string title,
            string content,
            string url,
            DateTime publishDate,
            uint occurenceInTitle,
            uint occurenceInLinks,
            IEnumerable<string> linkTexts,
            string word,
            uint wordFrequencyInFile,
            List<uint> positions)
        {
            if (title == null)
            {
                title = "";
            }

            if (content == null)
            {
                content = "";
            }

            var linkCount = linkTexts.Count();

            double score;

            if (positions.Any())
            {
                score = (occurenceInTitle * 3 * word.Length) / (1 + title.Length)
                    + (occurenceInLinks * 5) / (1 + linkCount)
                    + wordFrequencyInFile * word.Length / (1 + content.Length)
                    + 1 - (1 + positions.First()) / (1 + content.Length)
                    + Math.Exp(-UrlHelper.GetDomainDepth(url))
                    + Math.Exp(-Math.Max(0, DateTime.Now.Subtract(publishDate).TotalDays / 30 - 3));
            }
            else
            {
                score = (occurenceInTitle * 4 * word.Length) / (1 + title.Length)
                    + (occurenceInLinks * 6) / (1 + linkCount)
                    + Math.Exp(-UrlHelper.GetDomainDepth(url))
                    + Math.Exp(-Math.Max(0, DateTime.Now.Subtract(publishDate).TotalDays / 30 - 3));
            }

            return score;
        }
    }
}
