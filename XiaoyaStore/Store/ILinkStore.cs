using System;
using System.Collections.Generic;
using XiaoyaStore.Model;

namespace XiaoyaStore.Store
{
    public interface ILinkStore : IDisposable
    {
        IList<Link> GetLinks(string url);
        bool ClearLinks(ulong urlFileId);
        bool SaveLinks(ulong urlFileId, IEnumerable<Link> links);
    }
}