using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaCommon.Data.Crawler;
using XiaoyaCommon.Data.Crawler.Model;

namespace XiaoyaCommon.Store
{
    public class UrlFileStore : IUrlFileStore
    {
        public UrlFile LoadByFilePath(string path)
        {
            using (var context = new CrawlerContext())
            {
                return context.UrlFiles.Single(o => o.FilePath == path);
            }
        }

        public UrlFile LoadByUrl(string url)
        {
            using (var context = new CrawlerContext())
            {
                return context.UrlFiles.Single(o => o.Url == url);
            }
        }

        public async Task<UrlFile> SaveAsync(UrlFile urlFile)
        {
            using (var context = new CrawlerContext())
            {
                var oldUrlFile = context.UrlFiles.SingleOrDefault(o => o.Url == urlFile.Url);
                if (oldUrlFile != null)
                {
                    File.Delete(oldUrlFile.FilePath);
                    oldUrlFile.FilePath = urlFile.FilePath;
                    oldUrlFile.Charset = urlFile.Charset;
                    oldUrlFile.MimeType = urlFile.MimeType;
                }
                else
                {
                    await context.UrlFiles.AddAsync(urlFile);
                }
                await context.SaveChangesAsync();
                return urlFile;
            }
        }
    }
}
