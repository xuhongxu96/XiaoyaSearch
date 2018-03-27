using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaCommon.Helper
{
    public static class ScoringHelpers
    {
        public static double TfIdf(long frequency, long documentFrequency, int documentCount)
        {
            double idf = Math.Log(documentCount) - Math.Log(documentFrequency);
            return frequency * idf;
        }
    }
}
