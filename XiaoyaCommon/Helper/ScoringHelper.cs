using System;
using System.Collections.Generic;
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
            string word, 
            long wordFrequency, 
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

            if (content.Contains(word))
            {
                return (title.Contains(word) ? 5.0 * word.Length : 1.0) / (1 + title.Length)
                    + wordFrequency * word.Length / (1 + content.Length)
                    + Math.Exp(-UrlHelper.GetDomainDepth(url))
                    + 1 - (1 + minPosition) / (1 + content.Length)
                    + Math.Exp(-Math.Max(0, DateTime.Now.Subtract(publishDate).TotalDays / 30 - 3));
            }
            else
            {
                return (title.Contains(word) ? 5.0 * word.Length : 2.0) / (1 + title.Length)
                    + Math.Exp(-UrlHelper.GetDomainDepth(url))
                    + Math.Exp(-Math.Max(0, DateTime.Now.Subtract(publishDate).TotalDays / 30 - 3));
            }
        }
    }
}
