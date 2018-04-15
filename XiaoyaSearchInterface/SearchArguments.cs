﻿using System;
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

        [Argument("-DbType",
            Alias = "-d",
            DefaultValue = "sqlite",
            Required = true)]
        public string DbType { get; set; }

        [Argument("-DbConnectionString",
            Alias = "-c",
            DefaultValue = "Data Source=Db/XiaoyaSearch.db",
            Required = false)]
        public string DbConnectionString { get; set; }
    }
}