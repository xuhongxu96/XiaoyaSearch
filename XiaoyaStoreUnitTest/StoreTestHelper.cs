using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XiaoyaStore.Config;
using XiaoyaStore.Data;

namespace XiaoyaStoreUnitTest
{
    public class StoreTestHelper : IDisposable
    {
        protected StoreConfig mConfig;

        protected string DbFileName { get; private set; }

        protected string DbPath { get; private set; }

        public RocksDb Db { get; private set; }

        public static void DeleteDb(StoreConfig config, string DbFileName)
        {
            var dbPath = Path.Combine(config.StoreDirectory, DbFileName + ".db");
            if (Directory.Exists(dbPath))
                Directory.Delete(dbPath, true);
        }

        public StoreTestHelper(StoreConfig config, string DbFileName)
        {
            mConfig = config;
            this.DbFileName = DbFileName;

            DbPath = Path.Combine(config.StoreDirectory, DbFileName + ".db");
        }

        protected IEnumerable<T> GetModelsByIds<T>(IEnumerable<long> ids) where T : class
        {
            var pairs = Db.MultiGet(ids.Select(o => o.GetBytes()).ToArray());
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

        public void OpenDb(ColumnFamilies columnFamilies = null)
        {
            if (columnFamilies == null)
            {
                columnFamilies = new ColumnFamilies();
            }

            var options = new DbOptions()
                .SetCreateIfMissing(true)
                .SetCreateMissingColumnFamilies(true);

            Db = RocksDb.Open(options, DbPath, columnFamilies);
        }

        ~StoreTestHelper()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Db != null)
            {
                Db.Dispose();
                Db = null;
            }
        }
    }
}
