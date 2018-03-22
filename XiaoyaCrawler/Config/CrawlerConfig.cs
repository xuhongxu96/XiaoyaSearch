using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XiaoyaFileParser;
using XiaoyaStore.Store;

namespace XiaoyaCrawler.Config
{
    public class CrawlerConfig
    {
        public bool UsePhantomJS { get; set; } = false;
        public string PhantomJSDriverPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "../../../../Resources/");

        public string LogDirectory { get; set; } = "Logs";
        public string FetchDirectory { get; set; } = "Fetched";

        public int MaxFetchingConcurrency { get; set; } = 10;

        public IEnumerable<string> InitUrls { get; set; }

        public IUrlFileStore UrlFileStore { get; set; }
        public IUrlFrontierItemStore UrlFrontierItemStore { get; set; }
    }
}
