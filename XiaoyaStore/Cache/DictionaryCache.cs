using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace XiaoyaStore.Cache
{
    public class DictionaryCache<TKey, TValue>
    {
        public struct CacheData
        {
            public TValue value;
            public DateTime expiredTime;
        }

        protected Func<TKey, TValue> mGetValueMethod;
        protected Func<IEnumerable<Tuple<TKey, TValue>>> mLoadCachesMethod;
        protected TimeSpan mExpiredTime;

        protected ConcurrentDictionary<TKey, CacheData> mDictionary
            = new ConcurrentDictionary<TKey, CacheData>();

        protected Thread mLoadCachesThread = null;

        public DictionaryCache(TimeSpan expiredTime, 
            Func<TKey, TValue> getValueMethod, 
            Func<IEnumerable<Tuple<TKey, TValue>>> loadCachesMethod = null)
        {
            mExpiredTime = expiredTime;
            mGetValueMethod = getValueMethod;
            mLoadCachesMethod = loadCachesMethod;

            if (mLoadCachesMethod != null)
            {
                mLoadCachesThread = new Thread(LoadCaches)
                {
                    Priority = ThreadPriority.BelowNormal,
                };
                mLoadCachesThread.Start();
            }
        }

        public void LoadCaches()
        {
            foreach (var item in mLoadCachesMethod())
            {
                var data = new CacheData
                {
                    value = item.Item2,
                    expiredTime = DateTime.Now.Add(mExpiredTime),
                };
                mDictionary.AddOrUpdate(item.Item1, data, (k, v) => data);
            }
        }

        public TValue Get(TKey key)
        {
            if (mDictionary.TryGetValue(key, out CacheData value))
            {
                if (value.expiredTime > DateTime.Now)
                {
                    return value.value;
                }
            }

            var newValue = mGetValueMethod(key);
            var data = new CacheData
            {
                value = newValue,
                expiredTime = DateTime.Now.Add(mExpiredTime),
            };
            mDictionary.AddOrUpdate(key, data, (k, v) => data);

            return newValue;
        }
    }
}
