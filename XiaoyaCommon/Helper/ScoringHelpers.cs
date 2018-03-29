using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaCommon.Helper
{
    public static class ScoringHelpers
    {
        public static double TfIdf(long frequency, long documentFrequency, int documentCount)
        {
            double idf = Math.Log(documentCount + 1) - Math.Log(documentFrequency);
            return Math.Log(frequency + 1) * idf;
        }
    }
}
