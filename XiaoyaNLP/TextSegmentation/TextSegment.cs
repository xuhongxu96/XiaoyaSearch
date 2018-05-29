using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaNLP.TextSegmentation
{
    public class TextSegment
    {
        public string Word { get; set; }
        public uint Position { get; set; }
        public uint Length { get; set; }
    }
}
