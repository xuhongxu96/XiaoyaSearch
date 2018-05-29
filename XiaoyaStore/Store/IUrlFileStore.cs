using System;
using System.Collections.Generic;
using XiaoyaStore.Model;

namespace XiaoyaStore.Store
{
    public interface IUrlFileStore : IDisposable
    {
        bool FinishIndex(string url);
        ulong GetCount();
        UrlFile GetForIndex();
        UrlFile GetUrlFile(string url);
        UrlFile GetUrlFile(ulong id);
        IList<UrlFile> GetUrlFilesByHash(string hash);
        (UrlFile urlFile, ulong oldUrlFileId) SaveUrlFileAndGetOldId(UrlFile urlFile);
    }
}