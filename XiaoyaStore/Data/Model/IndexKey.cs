using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public partial class IndexKey
    {
        public override bool Equals(object obj)
        {
            var key = obj as IndexKey;
            return key != null &&
                   UrlFileId == key.UrlFileId &&
                   Word == key.Word;
        }

        public override int GetHashCode()
        {
            var hashCode = 2108208906;
            hashCode = hashCode * -1521134295 + UrlFileId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Word);
            return hashCode;
        }
    }
}
