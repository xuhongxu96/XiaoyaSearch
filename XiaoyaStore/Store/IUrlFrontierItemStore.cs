using System.Collections.Generic;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface IUrlFrontierItemStore
    {
        int Count();
        void Init(IEnumerable<string> initUrls);
        void Restart();
        UrlFrontierItem PopUrlForCrawl(bool strictPlannedTime);
        UrlFrontierItem LoadByUrl(string url);
        UrlFrontierItem Save(string url);
        UrlFrontierItem PushBack(string url);
    }
}