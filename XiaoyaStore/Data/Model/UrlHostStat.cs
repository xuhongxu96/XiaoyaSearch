using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class UrlHostStat
    {
        public int UrlHostStatId { get; set; }
        public string Host { get; set; }
        public int Count { get; set; }
    }
}
