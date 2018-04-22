using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class InvertedIndex
    {
        public int InvertedIndexId { get; set; }
        [ConcurrencyCheck]
        [Column(TypeName = "nvarchar(30)")]
        public string Word { get; set; }
        [ConcurrencyCheck]
        public int UrlFileId { get; set; }
        [ConcurrencyCheck]
        public long WordFrequency { get; set; }
        [ConcurrencyCheck]
        public double Weight { get; set; }
        [ConcurrencyCheck]
        public string Positions { get; set; }
        [ConcurrencyCheck]
        public int OccurencesInTitle{ get; set; }
        [ConcurrencyCheck]
        public int OccurencesInLinks { get; set; }

        public List<int> PositionArr
        {
            get => Positions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(o => int.Parse(o)).ToList();

        }
    }
}
