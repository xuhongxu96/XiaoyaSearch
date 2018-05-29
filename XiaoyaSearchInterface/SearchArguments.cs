using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaCommon.ArgumentParser;

namespace XiaoyaSearchInterface
{
    public class SearchArguments
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

    }
}
