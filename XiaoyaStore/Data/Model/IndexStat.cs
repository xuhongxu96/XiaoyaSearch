using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class IndexStat
    {
        public int IndexStatId { get; set; }
        public string Word { get; set; }
        public long WordFrequency { get; set; }
        public long DocumentFrequency { get; set; }
    }
}
