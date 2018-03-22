using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaRetriever
{
    public class RetrievedUrlFilePositions : IEnumerable<WordPosition>
    {
        public int UrlFileId { get; set; }
        protected HashSet<WordPosition> mPositions;

        public RetrievedUrlFilePositions(int urlFileId)
        {
            UrlFileId = urlFileId;
            mPositions = new HashSet<WordPosition>();
        }

        public RetrievedUrlFilePositions(int urlFileId, IEnumerable<WordPosition> positions)
        {
            UrlFileId = urlFileId;
            mPositions = new HashSet<WordPosition>(positions);
        }

        public IEnumerator<WordPosition> GetEnumerator()
        {
            return mPositions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mPositions.GetEnumerator();
        }

        public void Add(WordPosition position)
        {
            mPositions.Add(position);
        }

        public override bool Equals(object obj)
        {
            var positions = obj as RetrievedUrlFilePositions;
            return positions != null &&
                   UrlFileId == positions.UrlFileId;
        }

        public override int GetHashCode()
        {
            return -1830584083 + UrlFileId.GetHashCode();
        }
    }
}
