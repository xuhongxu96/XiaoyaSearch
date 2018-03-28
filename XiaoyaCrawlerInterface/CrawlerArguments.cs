using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaCommon.ArgumentParser;

namespace XiaoyaCrawlerInterface
{
    public class CrawlerArguments
    {
        [Argument("-InitUrls",
            Alias = "-i",
            DefaultValue = "http://www.bnu.edu.cn",
            Required = true)]
        public string InitUrl { get; set; }

        [Argument("-FetchDir",
            Alias = "-f",
            DefaultValue = "Fetch",
            Required = true)]
        public string FetchDir { get; set; }

        [Argument("-LogDir",
            Alias = "-l",
            DefaultValue = "Logs",
            Required = true)]
        public string LogDir { get; set; }

        [Argument("-DbDir",
            Alias = "-d",
            DefaultValue = "Db",
            Required = true)]
        public string DbDir { get; set; }
    }
}
