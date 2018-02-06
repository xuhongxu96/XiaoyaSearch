using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaCommon.Data.Crawler.Model
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
        public string FilePath { get; set; }
        /// <summary>
        /// Hash of fetched content file
        /// </summary>
        public string FileHash { get; set; }
        /// <summary>
        /// Fetched content charset
        /// </summary>
        public string Charset { get; set; }
        /// <summary>
        /// Fetched content MIME type
        /// </summary>
        public string MimeType { get; set; }
        /// <summary>
        /// DateTime offset between present and last update
        /// </summary>
        public TimeSpan UpdateInterval { get; set; }
        /// <summary>
        /// Updated at
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        /// <summary>
        /// Created at
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
