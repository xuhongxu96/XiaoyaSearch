using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaFileParser.Model
{
    public class Token
    {
        public string Word { get; set; }
        public int Length { get; set; }
        public List<int> Positions { get; set; }
        public int WordFrequency { get; set; }
        public int OccurenceInTitle { get; set; }
        public int OccurenceInLinks { get; set; }
    }
}
