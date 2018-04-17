using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Cache;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Helper;

namespace XiaoyaStore.Store
{
    public class UrlFileStore : BaseStore, IUrlFileStore
    {
        protected LRUCache<int, UrlFile> mCache;

        public UrlFileStore(DbContextOptions options = null, bool enableCache = true) : base(options)
        {
            mCache = new LRUCache<int, UrlFile>(TimeSpan.FromDays(1), GetCache, null, 100_000, enableCache);
        }

        protected UrlFile GetCache(int id)
        {
            using (var context = NewContext())
            {
                return context.UrlFiles.SingleOrDefault(o => o.UrlFileId == id);
            }
        }

        public int Count()
        {
            using (var context = NewContext())
            {
                return context.UrlFiles.Count();
            }
        }

        public void RestartIndex()
        {
            using (var context = NewContext())
            {
                context.Database.ExecuteSqlCommand("UPDATE UrlFiles SET IndexStatus = 0 WHERE IndexStatus = 1");
            }
        }

        public UrlFile LoadAnyForIndex()
        {
            using (var context = NewContext())
            {
                UrlFile urlFile;
                if (context.Database.IsSqlServer())
                {
                    urlFile = context.UrlFiles
                        .FromSql("SELECT TOP 1 * FROM UrlFiles WHERE IndexStatus = 0 ORDER BY UpdatedAt")
                        .FirstOrDefault();
                }
                else
                {
                    urlFile = context.UrlFiles
                        .Where(o => o.IndexStatus == UrlFile.UrlFileIndexStatus.NotIndexed)
                        .OrderBy(o => o.UpdatedAt)
                        .FirstOrDefault();
                }

                if (urlFile == null)
                {
                    return null;
                }

                urlFile.IndexStatus = UrlFile.UrlFileIndexStatus.Indexing;

                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return null;
                }

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

        public UrlFile LoadById(int id)
        {
            return mCache.Get(id);
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
                    urlFile.IndexStatus = UrlFile.UrlFileIndexStatus.NotIndexed;
                    urlFile.UpdatedAt = DateTime.Now;
                    urlFile.CreatedAt = DateTime.Now;
                    urlFile.UpdateInterval = TimeSpan.FromDays(5);
                    context.UrlFiles.Add(urlFile);
                }
                else
                {
                    // Exists this url, then judge if two fetched file is same
                    if ((urlFile.Title != "" && oldUrlFile.Title != urlFile.Title)
                            || (urlFile.Content != "" && oldUrlFile.Content != urlFile.Content))
                    {
                        // Updated
                        var updateInterval = DateTime.Now.Subtract(oldUrlFile.UpdatedAt);

                        oldUrlFile.UpdatedAt = DateTime.Now;
                        oldUrlFile.UpdateInterval
                            = (oldUrlFile.UpdateInterval * 3 + updateInterval) / 4;

                        oldUrlFile.IndexStatus = UrlFile.UrlFileIndexStatus.NotIndexed;
                    }

                    // Delete old file
                    File.Delete(oldUrlFile.FilePath);

                    // Update info
                    oldUrlFile.FilePath = urlFile.FilePath;
                    oldUrlFile.FileHash = urlFile.FileHash;
                    oldUrlFile.Title = urlFile.Title;
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

        public IEnumerable<UrlFile> LoadByHash(string hash)
        {
            using (var context = NewContext())
            {
                foreach (var item in context.UrlFiles.Where(o => o.FileHash == hash))
                {
                    yield return item;
                }
            }
        }

        public UrlFile UpdateUrl(int id, string url)
        {
            using (var context = NewContext())
            {
                var urlFile = context.UrlFiles.Single(o => o.UrlFileId == id);

                var sameUrl = context.UrlFiles.SingleOrDefault(o => o.Url == url);
                if (sameUrl != null)
                {
                    context.UrlFiles.Remove(urlFile);
                }
                else
                {
                    urlFile.Url = url;
                }
                context.SaveChanges();

                return urlFile;
            }
        }
    }
}
