using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Cache;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public class IndexStatStore : BaseStore, IIndexStatStore
    {
        protected LRUCache<string, IndexStat> mCache;

        public IndexStatStore(DbContextOptions options = null, bool enableCache = true) : base(options)
        {
            mCache = new LRUCache<string, IndexStat>(TimeSpan.FromDays(5), GetCache, LoadCaches, 1_000_000, enableCache);
        }

        public IndexStat LoadByWord(string word)
        {
            var indexStat = mCache.Get(word);

            if (indexStat == null)
            {
                return null;
            }

            if (indexStat.DocumentFrequency < 0)
            {
                indexStat.DocumentFrequency = 0;
            }

            if (indexStat.WordFrequency < 0)
            {
                indexStat.WordFrequency = 0;
            }

            return indexStat;
        }

        protected IEnumerable<Tuple<string, IndexStat>> LoadCaches()
        {
            using (var context = NewContext())
            {
                foreach (var item in context.IndexStats.OrderByDescending(o => o.WordFrequency))
                {
                    if (mCache.IsValid(item.Word))
                    {
                        continue;
                    }

                    yield return Tuple.Create(item.Word, item);
                }
            }
        }

        protected IndexStat GetCache(string word)
        {
            using (var context = NewContext())
            {
                return context.IndexStats.AsNoTracking().SingleOrDefault(o => o.Word == word);
            }
        }
    }
}
