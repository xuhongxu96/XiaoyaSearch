using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaCrawler.Fetcher
{
    public class FetchedFile
    {
        public string Url { get; set; }
        public string FilePath { get; set; }
        public string Charset { get; set; }
        public string MimeType { get; set; }
        public string FileHash { get; set; }
    }
}
