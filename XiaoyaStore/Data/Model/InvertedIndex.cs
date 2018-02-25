using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class InvertedIndex
    {
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
    }
}
