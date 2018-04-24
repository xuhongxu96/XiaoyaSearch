using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using XiaoyaStore.Data.Model;
using System.Linq;
using XiaoyaStore.Cache;
using EFCore.BulkExtensions;

namespace XiaoyaStore.Store
{
    public class LinkStore : BaseStore, ILinkStore
    {
        protected LRUCache<string, List<Link>> mCache;

        static object mSyncLock = new object();

        public LinkStore(DbContextOptions options = null, bool enableCache = true) : base(options)
        {
            mCache = new LRUCache<string, List<Link>>(TimeSpan.FromDays(1), GetCache, null, 30_000_000, enableCache);
        }

        protected IEnumerable<Tuple<string, List<Link>>> LoadCaches()
        {
            using (var context = NewContext())
            {
                foreach (var link in context.Links.GroupBy(o => o.Url))
                {
                    yield return Tuple.Create(link.Key, link.ToList());
                }
            }
        }

        protected List<Link> GetCache(string url)
        {
            using (var context = NewContext())
            {
                return context.Links.Where(o => o.Url == url).ToList();
            }
        }


        public void ClearAndSaveLinksForUrlFile(int urlFileId, IEnumerable<Link> links)
        {
            using (var context = NewContext())
            {
                var list = new List<Link>();
                foreach (var link in context.Links.Where(o => o.UrlFileId == urlFileId))
                {
                    list.Add(link);
                    if (list.Count % 10000 == 0)
                    {
                        if (context.Database.IsSqlServer())
                        {
                            context.BulkDelete(list);
                        }
                        else
                        {
                            context.RemoveRange(list);
                        }
                        list.Clear();
                    }
                }
                if (list.Count > 0)
                {
                    if (context.Database.IsSqlServer())
                    {
                        context.BulkDelete(list);
                    }
                    else
                    {
                        context.RemoveRange(list);
                    }
                }

                list.Clear();
                foreach (var link in links)
                {
                    list.Add(link);
                    if (list.Count % 10000 == 0)
                    {
                        if (context.Database.IsSqlServer())
                        {
                            context.BulkInsert(list);
                        }
                        else
                        {
                            context.AddRange(list);
                        }
                        list.Clear();
                    }
                }
                if (list.Count > 0)
                {
                    if (context.Database.IsSqlServer())
                    {
                        context.BulkInsert(list);
                    }
                    else
                    {
                        context.AddRange(list);
                    }
                }

                lock (mSyncLock)
                {
                    context.SaveChanges();
                }
            }
        }

        public IEnumerable<Link> LoadByUrl(string url)
        {
            return mCache.Get(url);
        }
    }
}
