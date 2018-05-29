using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XiaoyaNLP.Encoding;
using XiaoyaNLP.Helper;

namespace XiaoyaNLP.TextSegmentation
{
    public class JiebaSegmenter : ITextSegmenter
    {
        protected static JiebaNet.Segmenter.JiebaSegmenter sSegmenter 
            = new JiebaNet.Segmenter.JiebaSegmenter();
        protected const int MaxSegmentLength = 15;
        protected bool mIsIndex;

        public JiebaSegmenter(bool isIndex = true)
        {
            mIsIndex = isIndex;
        }

        protected IEnumerable<TextSegment> SegmentChineseSentence(string sentence, int baseIndex)
        {
            foreach (var token in sSegmenter.Tokenize(sentence, 
                mIsIndex ? JiebaNet.Segmenter.TokenizerMode.Search : JiebaNet.Segmenter.TokenizerMode.Default))
            {
                yield return new TextSegment
                {
                    Word = token.Word,
                    Position = (uint) (baseIndex + token.StartIndex),
                    Length = (uint) token.Word.Length,
                };
            }
        }

        public IEnumerable<TextSegment> Segment(string sentence)
        {
            var match = CommonRegex.AnyChars.Match(sentence);

            while (match.Success)
            {
                var subSentence = match.Value;
                var index = match.Index;

                if (CommonRegex.ChineseChars.IsMatch(subSentence))
                {
                    foreach (var segment in SegmentChineseSentence(subSentence, index))
                    {
                        if (segment.Length <= MaxSegmentLength)
                        {
                            yield return segment;
                        }
                    }
                }
                else
                {
                    if (subSentence.Length <= MaxSegmentLength)
                    {
                        yield return new TextSegment
                        {
                            Word = subSentence,
                            Position = (uint) index,
                            Length = (uint) subSentence.Length,
                        };
                    }
                }

                match = match.NextMatch();
            }

        }
    }
}
