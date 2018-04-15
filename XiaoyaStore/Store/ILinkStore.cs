﻿using System.Collections.Generic;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface ILinkStore
    {
        void ClearAndSaveLinksForUrlFile(int urlFileId, IEnumerable<Link> links);
        IEnumerable<Link> LoadByUrl(string url);
    }
}