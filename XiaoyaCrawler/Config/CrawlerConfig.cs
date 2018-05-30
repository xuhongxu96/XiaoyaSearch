using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using XiaoyaFileParser;
using XiaoyaStore.Store;

namespace XiaoyaCrawler.Config
{
    public class CrawlerConfig
    {
        public bool UsePhantomJS { get; set; } = false;
#if DEBUG
        public string PhantomJSDriverPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "../../../../Resources/");
#else
        public string PhantomJSDriverPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "../Resources/");
#endif

        public string LogDirectory { get; set; } = "Logs";
        public string FetchDirectory { get; set; } = "Fetched";

        public int MaxFetchingConcurrency { get; set; } = 10;

        public IEnumerable<string> InitUrls { get; set; }

        public Regex NotUseProxyUrlRegex { get; set; } = new Regex(@"bnu\.edu\.cn", RegexOptions.Compiled);

        public IUrlFileStore UrlFileStore { get; set; }
        public IUrlFrontierItemStore UrlFrontierItemStore { get; set; }
        public ILinkStore LinkStore { get; set; }
        public IPostingListStore PostingListStore { get; set; }
        public IInvertedIndexStore InvertedIndexStore { get; set; }
    }
}
