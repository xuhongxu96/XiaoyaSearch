using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using XiaoyaStore.Data.Model;
using System.Linq;
using XiaoyaStore.Cache;

namespace XiaoyaStore.Store
{
    public class LinkStore : BaseStore, ILinkStore
    {
        protected LRUCache<string, List<Link>> mCache;

        static object mSyncLock = new object();

        public LinkStore(DbContextOptions options = null) : base(options)
        {
            mCache = new LRUCache<string, List<Link>>(TimeSpan.FromDays(1), GetCache, null, 1_000_000);
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
                context.Links.RemoveRange(context.Links.Where(o => o.UrlFileId == urlFileId));
                context.Links.AddRange(links);
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
