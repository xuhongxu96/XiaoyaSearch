namespace XiaoyaStore.Store
{
    public interface ISameUrlStore
    {
        void Save(string rawUrl, string url);
    }
}