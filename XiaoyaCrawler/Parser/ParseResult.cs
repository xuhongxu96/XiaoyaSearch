using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaCrawler.Parser
{
    /// <summary>
    /// Parsing Result
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// Content of web page or file
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Urls in web page or file
        /// </summary>
        public IList<string> Urls { get; set; }
    }
}
