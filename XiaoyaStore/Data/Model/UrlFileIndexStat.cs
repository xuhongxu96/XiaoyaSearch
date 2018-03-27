using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class UrlFileIndexStat
    {
        public int UrlFileIndexStatId { get; set; }
        public int UrlFileId { get; set; }
        public string Word { get; set; }
        public long WordFrequency { get; set; }
    }
}
