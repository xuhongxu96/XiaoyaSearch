using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using XiaoyaStore.Data.Model;
using System.Linq;

namespace XiaoyaStore.Store
{
    public class LinkStore : BaseStore, ILinkStore
    {
        public LinkStore(DbContextOptions options = null) : base(options)
        {
        }

        public void ClearAndSaveLinksForUrlFile(int urlFileId, IEnumerable<Link> links)
        {
            using (var context = NewContext())
            {
                context.RemoveRange(context.Links.Where(o => o.UrlFileId == urlFileId));
                context.Links.AddRange(links);
                context.SaveChanges();
            }
        }
    }
}
