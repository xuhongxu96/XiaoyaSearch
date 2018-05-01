using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class SameUrl
    {
        public int SameUrlId { get; set; }
        [ConcurrencyCheck]
        [Column(TypeName = "nvarchar(300)")]
        public string RawUrl { get; set; }
        [Column(TypeName = "nvarchar(300)")]
        public string Url { get; set; }
    }
}
