using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Store;

namespace XiaoyaCrawler.Config
{
    public class CrawlerConfig
    {
        public string LogDirectory { get; set; } = "Logs";
        public string FetchDirectory { get; set; } = "Fetched";

        public string CheckPointDirectory { get; set; } = "CheckPoint";

        public int MaxFetchingConcurrency { get; set; } = 10;

        public IEnumerable<string> InitUrls { get; set; }

        public IUrlFileStore UrlFileStore { get; set; }
    }
}
