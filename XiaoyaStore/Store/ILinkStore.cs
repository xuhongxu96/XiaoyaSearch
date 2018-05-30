using System;
using System.Collections.Generic;
using XiaoyaStore.Model;

namespace XiaoyaStore.Store
{
    public interface ILinkStore : IDisposable
    {
        IList<Link> GetLinks(string url);
        bool SaveLinks(IEnumerable<Link> links);
        bool RemoveLinks(IEnumerable<Link> links);
    }
}