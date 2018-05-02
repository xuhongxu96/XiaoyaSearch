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

        protected IEnumerable<TextSegment> SegmentChineseSentence(string sentence, int baseIndex)
        {
            foreach (var token in sSegmenter.Tokenize(sentence, JiebaNet.Segmenter.TokenizerMode.Search))
            {
                yield return new TextSegment
                {
                    Word = token.Word,
                    Position = baseIndex + token.StartIndex,
                    Length = token.Word.Length,
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
                            Position = index,
                            Length = subSentence.Length,
                        };
                    }
                }

                match = match.NextMatch();
            }

        }
    }
}
