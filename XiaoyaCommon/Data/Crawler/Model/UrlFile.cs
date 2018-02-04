using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaCommon.Data.Crawler.Model
{
    public class UrlFile
    {
        public int UrlFileId { get; set; }
        public string Url { get; set; }
        public string FilePath { get; set; }
        public string Charset { get; set; }
        public string MimeType { get; set; }
    }
}
