using System.Collections.Generic;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface IUrlFileIndexStatStore
    {
        IEnumerable<UrlFileIndexStat> LoadByWord(string word);
        UrlFileIndexStat LoadByWordInUrlFile(UrlFile urlFile, string word);
        UrlFileIndexStat LoadByWordInUrlFile(int urlFileId, string word);
        int CountWordInUrlFile(int urlFileId);
        int CountWordInUrlFile(UrlFile urlFile);
    }
}