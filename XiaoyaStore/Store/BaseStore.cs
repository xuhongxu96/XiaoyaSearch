using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XiaoyaStore.Config;
using XiaoyaStore.Data;
using XiaoyaStore.Data.MergeOperator;

namespace XiaoyaStore.Store
{
    public abstract class BaseStore : IDisposable
    {
        protected StoreConfig mConfig;
        protected bool mIsReadOnly;

        public abstract string DbFileName { get; }

        protected string DbPath { get; private set; }

        protected RocksDb mDb;

        public BaseStore(StoreConfig config, bool isReadOnly)
        {
            mConfig = config;
            mIsReadOnly = isReadOnly;

            if (!Directory.Exists(config.StoreDirectory))
            {
                Directory.CreateDirectory(config.StoreDirectory);
            }

            DbPath = Path.Combine(config.StoreDirectory, DbFileName + ".db");
        }

        protected IEnumerable<T> GetModelsByIds<T>(IEnumerable<long> ids) where T : class
        {
            var pairs = mDb.MultiGet(ids.Select(o => o.GetBytes()).ToArray());
            if (pairs == null)
            {
                yield break;
            }

            foreach (var pair in pairs)
            {
                if (pair.Value == null)
                {
                    yield return default;
                }
                else
                {
                    var item = ModelSerializer.DeserializeModel<T>(pair.Value);
                    yield return item;
                }
            }
        }

        public virtual void OpenDb(ColumnFamilies columnFamilies = null)
        {
            if (columnFamilies == null)
            {
                columnFamilies = new ColumnFamilies();
            }

            var options = new DbOptions()
                .SetCreateIfMissing(true)
                .SetCreateMissingColumnFamilies(true);

            if (mIsReadOnly)
            {
                mDb = RocksDb.OpenReadOnly(options, DbPath, columnFamilies, false);
            }
            else
            {
                mDb = RocksDb.Open(options, DbPath, columnFamilies);
            }
        }

        ~BaseStore()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            if (mDb != null)
            {
                mDb.Dispose();
                mDb = null;
            }
        }
    }
}
