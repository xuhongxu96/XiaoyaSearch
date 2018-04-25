using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaNLP.WordStemmer
{
    public struct StemmedWord
    {
        public readonly string Value;

        public readonly string Unstemmed;

        public StemmedWord(string value, string unstemmed)
        {
            Value = value;
            Unstemmed = unstemmed;
        }
    }
}
