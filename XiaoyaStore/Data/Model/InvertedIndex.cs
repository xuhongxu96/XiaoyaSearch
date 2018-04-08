using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [ConcurrencyCheck]
        [Column(TypeName = "nvarchar(30)")]
        public string Word { get; set; }
        /// <summary>
        /// Id of UrlFile in which the word occurs
        /// </summary>
        [ConcurrencyCheck]
        public int UrlFileId { get; set; }
        /// <summary>
        /// Position the word occurs in the UrlFile
        /// </summary>
        [ConcurrencyCheck]
        public int Position { get; set; }
        /// <summary>
        /// Type
        /// </summary>
        [ConcurrencyCheck]
        public InvertedIndexType IndexType { get; set; } = InvertedIndexType.Body;
    }
}
