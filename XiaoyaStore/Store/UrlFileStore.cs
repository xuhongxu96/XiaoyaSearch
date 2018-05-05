using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Cache;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Helper;
using Z.EntityFramework.Plus;

namespace XiaoyaStore.Store
{
    public class UrlFileStore : BaseStore, IUrlFileStore
    {
        protected LRUCache<int, UrlFile> mCache;

        public UrlFileStore(DbContextOptions options = null, bool enableCache = true) : base(options)
        {
            mCache = new LRUCache<int, UrlFile>(TimeSpan.FromDays(1), GetCache, null, 500_000, enableCache);
        }

        protected UrlFile GetCache(int id)
        {
            using (var context = NewContext())
            {
                return context.UrlFiles.SingleOrDefault(o => o.UrlFileId == id);
            }
        }

        public void CacheUrlFiles(IEnumerable<int> urlFileIds)
        {
            var urlFileIdSet = urlFileIds.ToHashSet();
            foreach (var id in urlFileIds)
            {
                if (mCache.IsValid(id))
                {
                    urlFileIdSet.Remove(id);
                }
            }

            mCache.LoadCaches(() => LoadCachesOfUrlFiles(urlFileIdSet));
        }

        protected IEnumerable<Tuple<int, UrlFile>> LoadCachesOfUrlFiles(IEnumerable<int> urlFileIds)
        {
            using (var context = NewContext())
            {
                var urlFileIdSet = new HashSet<int>(urlFileIds);
                foreach (var item in context.UrlFiles
                    .Where(o => urlFileIdSet.Contains(o.UrlFileId))
                    /*.OrderByDescending(o => o.PageRank)*/)
                {
                    if (mCache.IsValid(item.UrlFileId))
                    {
                        continue;
                    }

                    yield return Tuple.Create(item.UrlFileId, item);
                }
            }
        }

        public int Count()
        {
            using (var context = NewContext())
            {
                return context.UrlFiles.Max(o => o.UrlFileId);
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

        public UrlFile Save(UrlFile urlFile, bool isUpdateContent = true)
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
                    urlFile.UpdateInterval = TimeSpan.FromDays(1);
                    context.UrlFiles.Add(urlFile);
                }
                else
                {
                    if (isUpdateContent)
                    {
                        // Exists this url, then judge if two fetched file is same
                        if (oldUrlFile.Title != urlFile.Title
                                || oldUrlFile.Content != urlFile.Content)
                        {
                            // Updated
                            oldUrlFile.UpdatedAt = DateTime.Now;
                            oldUrlFile.IndexStatus = UrlFile.UrlFileIndexStatus.NotIndexed;
                        }
                        else
                        {
                            // No changes and already indexed, delete file
                            if (oldUrlFile.IndexStatus == UrlFile.UrlFileIndexStatus.Indexed
                                && File.Exists(urlFile.FilePath))
                            {
                                File.Delete(urlFile.FilePath);
                            }
                        }

                        // Update UpdateInterval
                        var updateInterval = DateTime.Now.Subtract(oldUrlFile.UpdatedAt);
                        oldUrlFile.UpdateInterval
                            = (oldUrlFile.UpdateInterval * 3 + updateInterval) / 4;

                    }

                    if (oldUrlFile.FilePath != urlFile.FilePath
                            && File.Exists(oldUrlFile.FilePath))
                    {
                        // Delete old file
                        File.Delete(oldUrlFile.FilePath);
                    }

                    // Update info
                    oldUrlFile.FilePath = urlFile.FilePath;
                    oldUrlFile.FileHash = urlFile.FileHash;
                    oldUrlFile.Title = urlFile.Title;
                    oldUrlFile.TextContent = urlFile.TextContent;
                    oldUrlFile.PublishDate = urlFile.PublishDate;
                    oldUrlFile.Charset = urlFile.Charset;
                    oldUrlFile.MimeType = urlFile.MimeType;
                    oldUrlFile.HeaderCount = urlFile.HeaderCount;
                    oldUrlFile.HeaderTotalLength = urlFile.HeaderTotalLength;
                    oldUrlFile.LinkCount = urlFile.LinkCount;
                    oldUrlFile.LinkTotalLength = urlFile.LinkTotalLength;

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

        public void ReCrawl(UrlFile urlFile)
        {
            using (var context = NewContext())
            {
                context.UrlFiles.Where(o => o.UrlFileId == urlFile.UrlFileId)
                    .Update(o => new UrlFile
                    {
                        TextContent = "",
                    });

                context.UrlFrontierItems.Where(o => o.Url == urlFile.Url)
                    .Update(o => new UrlFrontierItem
                    {
                        PlannedTime = DateTime.Now,
                    });

                context.SaveChanges();
            }
        }

        public IEnumerable<(string url, string textContent)> LoadByHash(string hash)
        {
            using (var context = NewContext())
            {
                foreach (var item in context.UrlFiles
                    .Where(o => o.FileHash == hash)
                    .Select(o => Tuple.Create(o.Url, o.TextContent)))
                {
                    yield return (item.Item1, item.Item2);
                }
            }
        }
    }
}
