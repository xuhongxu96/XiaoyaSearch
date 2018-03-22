using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaRetriever
{
    public class WordPosition : IComparable
    {
        public string Word { get; set; }
        public int Position { get; set; }

        public int CompareTo(object obj)
        {
            return Position.CompareTo(obj);
        }

        public override bool Equals(object obj)
        {
            var position = obj as WordPosition;
            return position != null &&
                   Position == position.Position;
        }

        public override int GetHashCode()
        {
            return -425505606 + Position.GetHashCode();
        }
    }
}
