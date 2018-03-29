using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class IndexStat
    {
        public int IndexStatId { get; set; }
        [ConcurrencyCheck]
        public string Word { get; set; }
        [ConcurrencyCheck]
        public long WordFrequency { get; set; }
        [ConcurrencyCheck]
        public long DocumentFrequency { get; set; }
    }
}
