﻿using System.Collections.Generic;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface IUrlFrontierItemStore
    {
        int Count();
        void Init(IEnumerable<string> initUrls);
        void RestartCrawl();
        UrlFrontierItem PopUrlForCrawl(bool strictPlannedTime = true);
        UrlFrontierItem LoadByUrl(string url);
        UrlFrontierItem Push(string url);
        UrlFrontierItem PushBack(string url, bool failed = false);
    }
}