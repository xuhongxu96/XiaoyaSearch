using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace XiaoyaStore.Cache
{
    public class ComparableLRUCache<TKey, TValue> : LRUCache<TKey, TValue>
    {
        protected Func<TKey, TValue, bool> mComparerMethod;
        protected Func<TKey, TValue, TValue> mUpdateValueMethod;

        public ComparableLRUCache(TimeSpan expiredTime,
            Func<TKey, TValue> getValueMethod,
            Func<TKey, TValue, TValue> updateValueMethod,
            Func<TKey, TValue, bool> comparerMethod,
            Func<IEnumerable<Tuple<TKey, TValue>>> loadCachesMethod = null,
            int LRUSize = 10000, bool isEnabled = true)
            : base(expiredTime, getValueMethod, loadCachesMethod, LRUSize, isEnabled)
        {
            mComparerMethod = comparerMethod;
            mUpdateValueMethod = updateValueMethod;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override TValue Get(TKey key)
        {
            if (!mIsEnabled)
            {
                return mGetValueMethod(key);
            }

            TValue newValue;

            if (mDictionary.TryGetValue(key, out CacheData value))
            {
                mLruList.Remove(value.lruNode);
                if (value.expiredTime > DateTime.Now)
                {
                    if (mComparerMethod(key, value.value))
                    {
                        mLruList.AddLast(value.lruNode);
                        return value.value;
                    }
                    else
                    {
                        mDictionary.Remove(key);
                        newValue = mUpdateValueMethod(key, value.value);
                        AddItem(key, newValue, value.expiredTime);
                        return newValue;
                    }
                }
                mDictionary.Remove(key);
            }

            newValue = mGetValueMethod(key);
            AddItem(key, newValue);

            return newValue;
        }
    }
}
