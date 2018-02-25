using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaNLP.TextSegmentation
{
    public class TextSegment
    {
        public string Text { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }
    }
}
