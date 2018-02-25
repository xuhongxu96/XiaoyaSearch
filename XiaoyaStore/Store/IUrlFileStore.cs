using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface IUrlFileStore
    {
        Task<UrlFile> SaveAsync(UrlFile urlFile);
        Task<UrlFile> SaveContentAsync(int urlFileId, string content);
        UrlFile LoadByUrl(string url);
        UrlFile LoadByFilePath(string path);
        Task<UrlFile> LoadAnyForIndexAsync();
    }
}
