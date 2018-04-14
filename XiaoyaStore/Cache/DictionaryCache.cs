using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

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
        protected TimeSpan mExpiredTime;
        protected ConcurrentDictionary<TKey, CacheData> mDictionary
            = new ConcurrentDictionary<TKey, CacheData>();

        public DictionaryCache(TimeSpan expiredTime, Func<TKey, TValue> getValueMethod)
        {
            mExpiredTime = expiredTime;
            mGetValueMethod = getValueMethod;
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
