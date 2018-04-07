using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class UrlFileIndexStat
    {
        public int UrlFileIndexStatId { get; set; }
        [ConcurrencyCheck]
        public int UrlFileId { get; set; }
        [ConcurrencyCheck]
        public string Word { get; set; }
        [ConcurrencyCheck]
        public long WordFrequency { get; set; }
        public double Weight { get; set; }
    }
}
