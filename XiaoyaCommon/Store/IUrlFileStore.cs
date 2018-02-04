using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaCommon.Data.Crawler.Model;

namespace XiaoyaCommon.Store
{
    public interface IUrlFileStore
    {
        Task<UrlFile> SaveAsync(UrlFile urlFile);
        UrlFile LoadByUrl(string url);
        UrlFile LoadByFilePath(string path);
    }
}
