using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public class IndexStatStore : BaseStore, IIndexStatStore
    {
        protected ConcurrentDictionary<string, IndexStat> mStats = new ConcurrentDictionary<string, IndexStat>();
        protected bool mIsLoading = false;
        protected object mSyncLock = new object();

        public IndexStatStore(DbContextOptions options = null) : base(options)
        {
            LoadIntoMemory();
        }

        protected void LoadIntoMemory()
        {
            lock (mSyncLock)
            {
                if (mIsLoading)
                {
                    return;
                }
                mIsLoading = true;
            }

            using (var context = NewContext())
            {
                foreach (var stat in context.IndexStats)
                {
                    mStats.AddOrUpdate(stat.Word, stat, (k, v) => stat);
                }
            }

            lock (mSyncLock)
            {
                mIsLoading = false;
            }
        }

        protected async void LoadIntoMemoryAsync()
        {
            lock (mSyncLock)
            {
                if (mIsLoading)
                {
                    return;
                }
            }
            await Task.Run(() => { LoadIntoMemory(); });
        }

        public IndexStat LoadByWord(string word)
        {
            if (mStats.ContainsKey(word))
            {
                return mStats[word];
            }
            else
            {
                LoadIntoMemoryAsync();
                return null;
            }
        }
    }
}
