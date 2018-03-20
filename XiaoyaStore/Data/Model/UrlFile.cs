using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class UrlFile
    {
        /// <summary>
        /// ID
        /// </summary>
        public int UrlFileId { get; set; }
        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Fetched content file path
        /// </summary>
        [ConcurrencyCheck]
        public string FilePath { get; set; }
        /// <summary>
        /// Hash of fetched content file
        /// </summary>
        [ConcurrencyCheck]
        public string FileHash { get; set; }
        /// <summary>
        /// Text content of file
        /// </summary>
        [ConcurrencyCheck]
        public string Content { get; set; } = null;
        /// <summary>
        /// Fetched content charset
        /// </summary>
        [ConcurrencyCheck]
        public string Charset { get; set; }
        /// <summary>
        /// Fetched content MIME type
        /// </summary>
        [ConcurrencyCheck]
        public string MimeType { get; set; }
        /// <summary>
        /// Is Indexed
        /// </summary>
        [ConcurrencyCheck]
        public bool IsIndexed { get; set; }
        /// <summary>
        /// DateTime offset between present and last update
        /// </summary>
        [ConcurrencyCheck]
        public TimeSpan UpdateInterval { get; set; }
        /// <summary>
        /// Updated at
        /// </summary>
        [ConcurrencyCheck]
        public DateTime UpdatedAt { get; set; }
        /// <summary>
        /// Created at
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
