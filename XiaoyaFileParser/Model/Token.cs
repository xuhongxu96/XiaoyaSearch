using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaFileParser.Model
{
    public class Token
    {
        public enum TokenType
        {
            Title, Body, Link
        }
        public string Text { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }
        public TokenType Type { get; set; }
    }
}
