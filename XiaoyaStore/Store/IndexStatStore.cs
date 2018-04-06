using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public class IndexStatStore : BaseStore, IIndexStatStore
    {
        protected ConcurrentDictionary<string, IndexStat> mStats = new ConcurrentDictionary<string, IndexStat>();

        public IndexStatStore(DbContextOptions options = null) : base(options)
        {
            LoadIntoMemory();
        }

        protected void LoadIntoMemory()
        {
            using (var context = NewContext())
            {
                foreach (var stat in context.IndexStats)
                {
                    mStats.AddOrUpdate(stat.Word, stat, (k, v) => stat);
                }
            }
        }

        public IndexStat LoadByWord(string word)
        {
            if (mStats.ContainsKey(word))
            {
                return mStats[word];
            }
            else
            {
                return null;
            }
        }
    }
}
