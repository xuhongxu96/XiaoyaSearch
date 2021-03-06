﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XiaoyaNLP.Encoding;
using XiaoyaNLP.Helper;

namespace XiaoyaNLP.TextSegmentation
{
    public class MaxMatchSegmenter : ITextSegmenter
    {
        protected const int MaxSegmentLength = 15;
#if DEBUG
        protected const string DictFileName = "../../../../Resources/30wdict_utf8.txt";
#else
        protected const string DictFileName = "../Resources/30wdict_utf8.txt";
#endif
        protected static HashSet<string> mDict = new HashSet<string>();
        protected static Dictionary<char, int> mMaxWordLength = new Dictionary<char, int>();

        static MaxMatchSegmenter()
        {
            using (var reader = new StreamReader(DictFileName))
            {
                while (!reader.EndOfStream)
                {
                    var word = reader.ReadLine().Trim();
                    mDict.Add(word);

                    if (!mMaxWordLength.ContainsKey(word[0])
                        || word.Length > mMaxWordLength[word[0]])
                    {
                        mMaxWordLength[word[0]] = word.Length;
                    }
                }
            }
        }

        protected IEnumerable<TextSegment> SegmentChineseSentence(string sentence, int baseIndex)
        {
            int pointer = 0;
            while (pointer < sentence.Length)
            {
                int candidateMaxLength = Math.Min(sentence.Length - pointer,
                    mMaxWordLength.ContainsKey(sentence[pointer]) ? mMaxWordLength[sentence[pointer]] : 1);
                for (int i = candidateMaxLength; i >= 1; --i)
                {
                    var candidate = sentence.Substring(pointer, i);
                    if (mDict.Contains(candidate) || i == 1)
                    {
                        yield return new TextSegment
                        {
                            Word = candidate,
                            Position = (uint)(baseIndex + pointer),
                            Length = (uint)i,
                        };
                        pointer += i;
                        break;
                    }
                }
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
                            Position = (uint)index,
                            Length = (uint)subSentence.Length,
                        };
                    }
                }

                match = match.NextMatch();
            }

        }
    }
}
