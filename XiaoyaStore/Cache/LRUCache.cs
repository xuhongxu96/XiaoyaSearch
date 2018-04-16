using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace XiaoyaStore.Cache
{
    public class LRUCache<TKey, TValue>
    {
        public struct CacheData
        {
            public TValue value;
            public DateTime expiredTime;
            public LinkedListNode<TKey> lruNode;
        }

        protected Func<TKey, TValue> mGetValueMethod;
        protected Func<IEnumerable<Tuple<TKey, TValue>>> mLoadCachesMethod;
        protected TimeSpan mExpiredTime;
        protected int mLruSize = 0;

        protected Dictionary<TKey, CacheData> mDictionary
            = new Dictionary<TKey, CacheData>();
        protected LinkedList<TKey> mLruList
            = new LinkedList<TKey>();

        protected bool mIsEnabled;

        protected Thread mLoadCachesThread = null;

        public LRUCache(TimeSpan expiredTime,
            Func<TKey, TValue> getValueMethod,
            Func<IEnumerable<Tuple<TKey, TValue>>> loadCachesMethod = null,
            int LRUSize = 0,
            bool isEnabled = true)
        {
            mExpiredTime = expiredTime;
            mGetValueMethod = getValueMethod;
            mLoadCachesMethod = loadCachesMethod;
            mLruSize = LRUSize;
            mIsEnabled = isEnabled;

            if (mIsEnabled && mLoadCachesMethod != null)
            {
                mLoadCachesThread = new Thread(LoadCaches)
                {
                    Priority = ThreadPriority.BelowNormal,
                };
                mLoadCachesThread.Start();
            }
        }

        protected void RemoveFirst()
        {
            var node = mLruList.First;
            mLruList.RemoveFirst();
            mDictionary.Remove(node.Value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void AddItem(TKey k, TValue v)
        {
            if (mDictionary.Count >= mLruSize)
            {
                RemoveFirst();
            }

            var node = new LinkedListNode<TKey>(k);
            var data = new CacheData
            {
                value = v,
                expiredTime = DateTime.Now.Add(mExpiredTime),
                lruNode = node,
            };

            mLruList.AddLast(node);
            mDictionary.Add(k, data);
        }

        protected void LoadCaches()
        {
            foreach (var item in mLoadCachesMethod().Take(mLruSize))
            {
                Add(item.Item1, item.Item2);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(TKey key, TValue value)
        {
            if (!mIsEnabled) return;

            if (mDictionary.TryGetValue(key, out CacheData oldValue))
            {
                mLruList.Remove(oldValue.lruNode);
                mDictionary.Remove(key);
            }

            AddItem(key, value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public TValue Get(TKey key)
        {
            if (!mIsEnabled)
            {
                return mGetValueMethod(key);
            }

            if (mDictionary.TryGetValue(key, out CacheData value))
            {
                mLruList.Remove(value.lruNode);
                if (value.expiredTime > DateTime.Now)
                {
                    mLruList.AddLast(value.lruNode);
                    return value.value;
                }
                else
                {
                    mDictionary.Remove(key);
                }
            }

            var newValue = mGetValueMethod(key);
            AddItem(key, newValue);

            return newValue;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool IsValid(TKey key)
        {
            if (!mIsEnabled) return false;

            if (mDictionary.TryGetValue(key, out CacheData value))
            {
                if (value.expiredTime > DateTime.Now)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
