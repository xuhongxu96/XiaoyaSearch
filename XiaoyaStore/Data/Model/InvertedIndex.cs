using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class InvertedIndex
    {
        public enum InvertedIndexType
        {
            Body, Title
        }

        /// <summary>
        /// Id
        /// </summary>
        public int InvertedIndexId { get; set; }
        /// <summary>
        /// Word
        /// </summary>
        public string Word { get; set; }
        /// <summary>
        /// Id of UrlFile in which the word occurs
        /// </summary>
        public int UrlFileId { get; set; }
        /// <summary>
        /// Position the word occurs in the UrlFile
        /// </summary>
        public int Position { get; set; }
        /// <summary>
        /// Type
        /// </summary>
        public InvertedIndexType IndexType { get; set; } = InvertedIndexType.Body;

        public override bool Equals(object obj)
        {
            var index = obj as InvertedIndex;
            return index != null &&
                   Word == index.Word &&
                   UrlFileId == index.UrlFileId &&
                   Position == index.Position &&
                   IndexType == index.IndexType;
        }

        public override int GetHashCode()
        {
            var hashCode = -1716764649;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Word);
            hashCode = hashCode * -1521134295 + UrlFileId.GetHashCode();
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + IndexType.GetHashCode();
            return hashCode;
        }
    }
}
