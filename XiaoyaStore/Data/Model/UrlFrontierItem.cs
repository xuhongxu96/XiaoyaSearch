using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class UrlFrontierItem
    {
        /// <summary>
        /// Id
        /// </summary>
        public int UrlFrontierItemId { get; set; }
        /// <summary>
        /// Url
        /// </summary>
        [ConcurrencyCheck]
        public string Url { get; set; }
        /// <summary>
        /// Host of url
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// Depth of url (Number of '/')
        /// </summary>
        public int UrlDepth { get; set; }
        /// <summary>
        /// Date time for next crawling this url
        /// </summary>
        [ConcurrencyCheck]
        public DateTime PlannedTime { get; set; }
        /// <summary>
        /// Priority (High value means low priority)
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// Times that failed to fetch this url
        /// </summary>
        public int FailedTimes { get; set; }
        /// <summary>
        /// Is this url popped from url frontier
        /// </summary>
        [ConcurrencyCheck]
        public bool IsPopped { get; set; }
        /// <summary>
        ///  Date time when this url is added to the url frontier again
        /// </summary>
        [ConcurrencyCheck]
        public DateTime UpdatedAt { get; set; }
        /// <summary>
        /// Date time when this url is added to the url frontier
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
