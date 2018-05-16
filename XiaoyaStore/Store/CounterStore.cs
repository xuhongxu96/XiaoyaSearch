using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XiaoyaStore.Config;
using XiaoyaStore.Data;
using XiaoyaStore.Data.MergeOperator;

namespace XiaoyaStore.Store
{
    public abstract class CounterStore : BaseStore
    {
        protected string CounterDbPath { get; private set; }

        protected RocksDb mCounterDb;

        protected object mCounterLock = new object();

        public CounterStore(StoreConfig config,
            bool isReadOnly)
            : base(config, isReadOnly)
        {
            CounterDbPath = Path.Combine(config.StoreDirectory, DbFileName + ".counter.db");
        }

        public override void OpenDb(ColumnFamilies columnFamilies = null)
        {
            base.OpenDb(columnFamilies);

            var options = new DbOptions()
                .SetCreateIfMissing(true)
                .SetMergeOperator(new CounterOperator());

            if (mIsReadOnly)
            {
                mCounterDb = RocksDb.OpenReadOnly(options, DbPath, false);
            }
            else
            {
                mCounterDb = RocksDb.Open(options, DbPath);
            }
        }

        protected long GetCount(byte[] key)
        {
            var data = mCounterDb.Get(key);
            if (data != null)
            {
                return BitConverter.ToInt64(data, 0);
            }
            else
            {
                return 0;
            }
        }

        protected void UpdateCount(byte[] key, long delta)
        {
            mCounterDb.Merge(key, delta.GetBytes());
        }

        protected long GetAndUpdateCount(byte[] key, long delta)
        {
            long result;
            lock (mCounterLock)
            {
                result = GetCount(key);
                mCounterDb.Merge(key, delta.GetBytes());
            }
            return result;
        }

        ~CounterStore()
        {
            Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();
            if (mCounterDb != null)
            {
                mCounterDb.Dispose();
                mCounterDb = null;
            }
        }
    }
}
