using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface IIndexStatStore
    {
        IndexStat LoadByWord(string word);
    }
}