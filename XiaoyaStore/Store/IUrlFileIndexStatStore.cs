using System.Collections.Generic;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface IUrlFileIndexStatStore
    {
        IEnumerable<UrlFileIndexStat> LoadByWord(string word);
    }
}