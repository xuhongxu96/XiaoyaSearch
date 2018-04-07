using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class UrlFile
    {
        public enum UrlFileIndexStatus
        {
            NotIndexed, Indexing, Indexed
        }

        /// <summary>
        /// ID
        /// </summary>
        public int UrlFileId { get; set; }
        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }
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
        public UrlFileIndexStatus IndexStatus { get; set; }
        /// <summary>
        /// Seconds offset between present and last update
        /// </summary>
        [ConcurrencyCheck]
        public double UpdateIntervalSeconds { get; set; }
        /// <summary>
        /// Timespan between present and last update
        /// </summary>
        [NotMapped]
        public TimeSpan UpdateInterval
        {
            get => TimeSpan.FromSeconds(UpdateIntervalSeconds);
            set
            {
                UpdateIntervalSeconds = value.TotalSeconds;
            }
        }
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
