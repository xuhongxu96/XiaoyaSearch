﻿using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaNLP.WordStemmer
{
    public interface IStemmer
    {
        /// <summary>
        /// Stem a word.
        /// </summary>
        /// <param name="word">Word to stem.</param>
        /// <returns>
        /// The stemmed word, with a reference to the original unstemmed word.
        /// </returns>
        StemmedWord Stem(string word);
    }
}
