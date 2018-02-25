using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaNLP.TextSegmentation
{
    public interface ITextSegmenter
    {
        IEnumerable<TextSegment> Segment(string sentence);
    }
}
