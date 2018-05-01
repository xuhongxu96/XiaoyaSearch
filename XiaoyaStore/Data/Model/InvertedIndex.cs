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
        public long WordFrequency { get; set; }
        public double Weight { get; set; }
        public string Positions { get; set; }
        public int OccurencesInTitle{ get; set; }
        public int OccurencesInLinks { get; set; }
        public int OccurencesInHeaders { get; set; }

        public List<int> PositionArr
        {
            get => Positions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(o => int.Parse(o)).ToList();

        }
    }
}
