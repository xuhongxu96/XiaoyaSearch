using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class IndexStat
    {
        public int IndexStatId { get; set; }
        [ConcurrencyCheck]
        [Column(TypeName ="nvarchar(30)")]
        public string Word { get; set; }
        [ConcurrencyCheck]
        public long WordFrequency { get; set; }
        [ConcurrencyCheck]
        public long DocumentFrequency { get; set; }
    }
}
