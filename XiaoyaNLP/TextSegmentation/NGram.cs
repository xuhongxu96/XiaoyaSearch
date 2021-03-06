﻿using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaNLP.Helper;

namespace XiaoyaNLP.TextSegmentation
{
    public class NGram : ITextSegmenter
    {

        protected int mGramCount;
        protected bool mOnlySegmentChinese;

        public NGram(int n = 2, bool onlySegmentChinese = false)
        {
            mGramCount = n;
            mOnlySegmentChinese = onlySegmentChinese;
        }

        public IEnumerable<TextSegment> Segment(string sentence)
        {
            var match = CommonRegex.AnyChars.Match(sentence);
            while (match.Success)
            {
                var subSentence = match.Value;
                var index = match.Index;

                if (CommonRegex.ChineseChars.IsMatch(subSentence) || !mOnlySegmentChinese)
                {
                    for (int i = 0; i <= subSentence.Length - mGramCount; ++i)
                    {
                        yield return new TextSegment
                        {
                            Word = subSentence.Substring(i, mGramCount),
                            Position = (uint)(index + i),
                            Length = (uint)mGramCount,
                        };
                    }
                }
                else
                {
                    yield return new TextSegment
                    {
                        Word = subSentence,
                        Position = (uint)index,
                        Length = (uint)subSentence.Length,
                    };
                }

                match = match.NextMatch();
            }
        }
    }
}