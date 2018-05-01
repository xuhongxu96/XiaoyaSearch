using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using XiaoyaStore.Data.Model;
using Z.EntityFramework.Plus;

namespace XiaoyaStore.Store
{
    public class SameUrlStore : BaseStore, ISameUrlStore
    {
        public SameUrlStore(DbContextOptions options = null) : base(options)
        { }

        public void Save(string rawUrl, string url)
        {
            using (var context = NewContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                if (context.SameUrls.Any(o => o.RawUrl == rawUrl))
                {
                    return;
                }

                context.SameUrls.Add(new SameUrl
                {
                    RawUrl = rawUrl,
                    Url = url,
                });

                context.Links.Where(o => o.Url == rawUrl).Update(link => new Link
                {
                    Url = url,
                });

                context.SaveChanges();
            }
        }
    }
}
