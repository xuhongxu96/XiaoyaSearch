using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaCommon.Data.Crawler;
using XiaoyaCommon.Data.Crawler.Model;
using XiaoyaCommon.Helper;

namespace XiaoyaCommon.Store
{
    public class UrlFileStore : IUrlFileStore
    {
        public UrlFile LoadByFilePath(string path)
        {
            using (var context = new CrawlerContext())
            {
                return context.UrlFiles.SingleOrDefault(o => o.FilePath == path);
            }
        }

        public UrlFile LoadByUrl(string url)
        {
            using (var context = new CrawlerContext())
            {
                return context.UrlFiles.SingleOrDefault(o => o.Url == url);
            }
        }

        public async Task<UrlFile> SaveAsync(UrlFile urlFile)
        {
            using (var context = new CrawlerContext())
            {
                // Find if the url exists
                var oldUrlFile = context.UrlFiles.SingleOrDefault(o => o.Url == urlFile.Url);
                if (oldUrlFile != null)
                {
                    var updateInterval = DateTime.Now.Subtract(oldUrlFile.UpdatedAt);
                    oldUrlFile.UpdateInterval = (oldUrlFile.UpdateInterval * 3 + updateInterval) / 4;

                    // Exists this url, then judge if two fetched file is same
                    if (oldUrlFile.FileHash == urlFile.FileHash
                        && FileHelper.FilesAreEqual(oldUrlFile.FilePath, urlFile.FilePath))
                    {
                        // Same
                        // Delete new file
                        File.Delete(urlFile.FilePath);
                    }
                    else
                    {
                        // Updated
                        oldUrlFile.UpdatedAt = DateTime.Now;

                        // Delete old file
                        File.Delete(oldUrlFile.FilePath);

                        // Update info
                        oldUrlFile.FilePath = urlFile.FilePath;
                        oldUrlFile.FileHash = urlFile.FileHash;
                        oldUrlFile.Charset = urlFile.Charset;
                        oldUrlFile.MimeType = urlFile.MimeType;
                    }

                    urlFile = oldUrlFile;
                }
                else
                {
                    // first see this url, add to database
                    urlFile.UpdatedAt = DateTime.Now;
                    urlFile.CreatedAt = DateTime.Now;
                    urlFile.UpdateInterval = TimeSpan.FromDays(3);
                    await context.UrlFiles.AddAsync(urlFile);
                }
                await context.SaveChangesAsync();
                return urlFile;
            }
        }
    }
}
