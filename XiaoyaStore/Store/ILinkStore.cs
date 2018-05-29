using System;
using System.Collections.Generic;
using XiaoyaStore.Model;

namespace XiaoyaStore.Store
{
    public interface ILinkStore : IDisposable
    {
        Link GetLink(ulong id);
        IList<Link> GetLinksByUrl(string url);
        bool SaveLinksOfUrlFile(ulong urlFileId, ulong oldUrlFileId, IEnumerable<Link> links);
    }
}