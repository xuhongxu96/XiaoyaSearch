using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Helper;

namespace XiaoyaCommon.Helper
{
    public static class ScoringHelper
    {
        public static double TfIdf(long frequency, long documentFrequency, int documentCount)
        {
            double idf = Math.Log(documentCount + 1) - Math.Log(documentFrequency);
            return Math.Log(frequency + 1) * idf;
        }

        public static double CalculateIndexWeight(string title,
            string content,
            string url,
            DateTime publishDate,
            int occurenceInTitle,
            int occurenceInLinks,
            IEnumerable<string> linkTexts,
            string word,
            long wordFrequency,
            long documentFrequency,
            int documentCount,
            int minPosition)
        {
            if (title == null)
            {
                title = "";
            }

            if (content == null)
            {
                content = "";
            }

            var linkTotalLength = linkTexts.Sum(o => o.Length);

            double idf = Math.Log(documentCount + 1) - Math.Log(documentFrequency);
            double score;
            if (content.Contains(word))
            {
                score = (occurenceInTitle * 3 * word.Length) / (1 + title.Length)
                    + (occurenceInLinks * 5 * word.Length) / (1 + linkTotalLength)
                    + wordFrequency * word.Length / (1 + content.Length)
                    + 1 - (1 + minPosition) / (1 + content.Length)
                    + Math.Exp(-UrlHelper.GetDomainDepth(url))
                    + Math.Exp(-Math.Max(0, DateTime.Now.Subtract(publishDate).TotalDays / 30 - 3));
            }
            else
            {
                score = (occurenceInTitle * 4 * word.Length) / (1 + title.Length)
                    + (occurenceInLinks * 6 * word.Length) / (1 + linkTotalLength)
                    + Math.Exp(-UrlHelper.GetDomainDepth(url))
                    + Math.Exp(-Math.Max(0, DateTime.Now.Subtract(publishDate).TotalDays / 30 - 3));
            }

            return score * idf;
        }
    }
}
