using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Helper;

namespace XiaoyaStore.Store
{
    public class UrlFileStore : BaseStore, IUrlFileStore
    {
        public UrlFileStore(DbContextOptions options = null) : base(options)
        { }

        public UrlFile LoadAnyForIndex()
        {
            using (var context = NewContext())
            {
                var urlFile = context.UrlFiles
                    .OrderBy(o => o.UpdatedAt)
                    .FirstOrDefault(o => o.IsIndexed == false);

                if (urlFile == null)
                {
                    return null;
                }

                urlFile.IsIndexed = true;
                context.SaveChanges();

                return urlFile;
            }
        }

        public UrlFile LoadByFilePath(string path)
        {
            using (var context = NewContext())
            {
                return context.UrlFiles.SingleOrDefault(o => o.FilePath == path);
            }
        }

        public UrlFile LoadByUrl(string url)
        {
            using (var context = NewContext())
            {
                return context.UrlFiles.SingleOrDefault(o => o.Url == url);
            }
        }

        public UrlFile Save(UrlFile urlFile)
        {
            using (var context = NewContext())
            {
                // Find if the url exists
                var oldUrlFile = context.UrlFiles.SingleOrDefault(o => o.Url == urlFile.Url);
                if (oldUrlFile == null)
                {
                    // first see this url, add to database
                    urlFile.IsIndexed = false;
                    urlFile.UpdatedAt = DateTime.Now;
                    urlFile.CreatedAt = DateTime.Now;
                    urlFile.UpdateInterval = TimeSpan.FromDays(3);
                    context.UrlFiles.Add(urlFile);
                }
                else
                {
                    // Exists this url, then judge if two fetched file is same
                    if (urlFile.Content != "" && oldUrlFile.Content != urlFile.Content)
                    {
                        // Updated
                        var updateInterval = DateTime.Now.Subtract(oldUrlFile.UpdatedAt);

                        oldUrlFile.UpdatedAt = DateTime.Now;
                        oldUrlFile.UpdateInterval
                            = (oldUrlFile.UpdateInterval * 3 + updateInterval) / 4;
                    }

                    // Delete old file
                    File.Delete(oldUrlFile.FilePath);

                    // Update info
                    oldUrlFile.IsIndexed = false;
                    oldUrlFile.FilePath = urlFile.FilePath;
                    oldUrlFile.FileHash = urlFile.FileHash;
                    oldUrlFile.Content = urlFile.Content;
                    oldUrlFile.Charset = urlFile.Charset;
                    oldUrlFile.MimeType = urlFile.MimeType;

                    urlFile = oldUrlFile;
                }

                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    File.Delete(urlFile.FilePath);

                    e.Entries.Single().Reload();
                    context.SaveChanges();
                }

                return urlFile;
            }
        }
    }
}
