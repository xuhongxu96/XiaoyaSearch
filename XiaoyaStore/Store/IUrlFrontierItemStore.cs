using System;
using System.Collections.Generic;

namespace XiaoyaStore.Store
{
    public interface IUrlFrontierItemStore : IDisposable
    {
        ulong GetHostCount(string host);
        bool Init(IEnumerable<string> urls);
        bool Reload();
        string PopUrl();
        bool PushBackUrl(string url, ulong updateInterval, bool failed = false);
        bool PushUrls(IEnumerable<string> urls);
        bool RemoveUrl(string url);
    }
}