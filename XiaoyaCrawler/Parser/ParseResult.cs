using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaFileParser.Model;

namespace XiaoyaCrawler.Parser
{
    /// <summary>
    /// Parsing Result
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// Title of web file
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Content of web file
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Urls in web file
        /// </summary>
        public IList<LinkInfo> Links { get; set; }
        /// <summary>
        /// Published Date
        /// </summary>
        public DateTime PublishDate { get; set; }
    }
}
