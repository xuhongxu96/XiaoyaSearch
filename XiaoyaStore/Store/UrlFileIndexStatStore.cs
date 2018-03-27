using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public class UrlFileIndexStatStore : BaseStore, IUrlFileIndexStatStore
    {
        public UrlFileIndexStatStore(DbContextOptions options = null) : base(options)
        { }

        public IEnumerable<UrlFileIndexStat> LoadByWord(string word)
        {
            using (var context = NewContext())
            {
                foreach(var stat in context.UrlFileIndexStats
                    .Where(o => o.Word == word)
                    .OrderByDescending(o => o.WordFrequency))
                {
                    yield return stat;
                }
            }
        }
    }
}
