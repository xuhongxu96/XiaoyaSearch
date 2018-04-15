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
        protected DictionaryCache<string, IndexStat> mCache;

        public IndexStatStore(DbContextOptions options = null) : base(options)
        {
            mCache = new DictionaryCache<string, IndexStat>(TimeSpan.FromDays(5), GetCache, LoadCaches);
        }

        public IndexStat LoadByWord(string word)
        {
            return mCache.Get(word);
        }

        protected IEnumerable<Tuple<string, IndexStat>> LoadCaches()
        {
            using (var context = NewContext())
            {
                foreach (var item in context.IndexStats)
                {
                    yield return Tuple.Create(item.Word, item);
                }
            }
        }

        protected IndexStat GetCache(string word)
        {
            using (var context = NewContext())
            {
                return context.IndexStats.SingleOrDefault(o => o.Word == word);
            }
        }
    }
}
