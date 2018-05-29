using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaFileParser.Model
{
    public class Token
    {
        public string Word { get; set; }
        public uint Length { get; set; }
        public List<uint> Positions { get; set; }
        public uint WordFrequency { get; set; }
        public uint OccurencesInTitle { get; set; }
        public uint OccurencesInLinks { get; set; }
        public uint OccurencesInHeaders { get; set; }
    }
}
