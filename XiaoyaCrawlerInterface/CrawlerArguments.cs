using IPValverde.ArgumentParser;
using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaCrawlerInterface
{
    public class CrawlerArguments
    {
        [Argument("-InitUrls",
            Alias = "-i",
            DefaultValue = "http://www.bnu.edu.cn",
            Name = "Initial Urls (Separated by comma without spaces)",
            Required = true)]
        public string InitUrl;

        [Argument("-FetchDir",
            Alias = "-f",
            DefaultValue = "Fetch",
            Name = "Directory to save fetched web files",
            Required = true)]
        public string FetchDir;

        [Argument("-LogDir",
            Alias = "-l",
            DefaultValue = "Logs",
            Name = "Directory to save logs",
            Required = true)]
        public string LogDir;

        [Argument("-DbDir",
            Alias = "-d",
            DefaultValue = "Db",
            Name = "Directory to save database",
            Required = true)]
        public string DbDir;
    }
}
