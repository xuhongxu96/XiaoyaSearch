using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface IUrlFileStore
    {
        UrlFile Save(UrlFile urlFile);
        void RestartIndex();
        UrlFile LoadById(int id);
        UrlFile LoadByUrl(string url);
        IEnumerable<UrlFile> LoadByHash(string hash);
        UrlFile LoadByFilePath(string path);
        UrlFile LoadAnyForIndex();
        UrlFile UpdateUrl(int id, string url);
        int Count();
    }
}
