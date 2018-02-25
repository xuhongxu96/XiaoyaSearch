using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaFileParser.Model
{
    public class Token
    {
        public string Text { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }
    }
}
