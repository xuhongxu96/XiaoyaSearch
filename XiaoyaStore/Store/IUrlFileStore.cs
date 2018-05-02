using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface IUrlFileStore
    {
        UrlFile Save(UrlFile urlFile, bool isUpdateContent = true);
        void RestartIndex();
        UrlFile LoadById(int id);
        UrlFile LoadByUrl(string url);
        IEnumerable<(string Url, string Content)> LoadByHash(string hash);
        UrlFile LoadByFilePath(string path);
        UrlFile LoadAnyForIndex();
        int Count();
        void CacheUrlFiles(IEnumerable<int> urlFileIds);
        void ReCrawl(UrlFile urlFile);
    }
}
