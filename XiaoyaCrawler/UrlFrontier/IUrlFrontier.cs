using System;
using System.Collections.Generic;

namespace XiaoyaCrawler.UrlFrontier
{
    public interface IUrlFrontier
    {
        string PopUrl();
        void PushBackUrl(string url, ulong updateInterval, bool failed = false);
        void PushUrls(IEnumerable<string> urls);
        void RemoveUrl(string url);
    }
}