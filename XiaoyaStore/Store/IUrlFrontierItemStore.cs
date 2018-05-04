using System.Collections.Generic;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface IUrlFrontierItemStore
    {
        void Init(IEnumerable<string> initUrls);
        void RestartCrawl();
        string PopUrlForCrawl();
        UrlFrontierItem LoadByUrl(string url);
        void PushUrls(IEnumerable<string> urls);
        UrlFrontierItem PushBack(string url, bool failed = false);
        void Remove(string url);
    }
}