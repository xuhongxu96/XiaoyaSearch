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

        public override bool Equals(object obj)
        {
            var stat = obj as UrlFileIndexStat;
            return stat != null &&
                   UrlFileId == stat.UrlFileId &&
                   Word == stat.Word &&
                   WordFrequency == stat.WordFrequency;
        }

        public override int GetHashCode()
        {
            var hashCode = 1041631203;
            hashCode = hashCode * -1521134295 + UrlFileId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Word);
            hashCode = hashCode * -1521134295 + WordFrequency.GetHashCode();
            return hashCode;
        }
    }
}
