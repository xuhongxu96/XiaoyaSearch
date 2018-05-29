using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaCommon.ArgumentParser;

namespace XiaoyaIndexerInterface
{
    public class IndexerArguments
    {
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

        [Argument("-ThreadCount",
            Alias = "-t",
            DefaultValue = "10",
            Required = true)]
        public string ThreadCount { get; set; }
    }
}
