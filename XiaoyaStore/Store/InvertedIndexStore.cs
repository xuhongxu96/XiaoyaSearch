using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;
using System.Data.SqlClient;
using XiaoyaStore.Helper;
using XiaoyaStore.Cache;
using System.Collections.Concurrent;
using XiaoyaLogger;
using System.Timers;
using System.Threading;
using XiaoyaStore.Config;
using RocksDbSharp;
using XiaoyaStore.Data.MergeOperator;

namespace XiaoyaStore.Store
{
    public class InvertedIndexStore : CounterStore
    {
        private static readonly byte[] MetaMaxIndexId = "MaxIndexId".GetBytes();

        public override string DbFileName => "Indices";

        ColumnFamilyHandle mIndexKeyIndexColumnFamily;
        ColumnFamilyHandle mUrlFileIdIndexColumnFamily;

        protected LRUCache<IndexKey, Index> mIndexCache;

        public InvertedIndexStore(StoreConfig config,
            bool isReadOnly = false,
            bool enableCache = true)
            : base(config, isReadOnly)
        {
            var columnFamilyOptions
                = new ColumnFamilyOptions().SetMergeOperator(new IdListConcatOperator());

            var columnFamilies = new ColumnFamilies
            {
                {"IndexKeyIndex", columnFamilyOptions },
                {"UrlFileIdIndex", columnFamilyOptions },
            };

            OpenDb(columnFamilies);

            mIndexKeyIndexColumnFamily = mDb.GetColumnFamily("IndexKeyIndex");
            mUrlFileIdIndexColumnFamily = mDb.GetColumnFamily("UrlFileIdIndex");

            mIndexCache = new LRUCache<IndexKey, Index>(
                TimeSpan.FromDays(5), GetIndexCache, null, 3_000_000, enableCache);
        }

        private Index GetIndex(long id)
        {
            var data = mDb.Get(id.GetBytes());
            if (data == null)
            {
                return null;
            }
            return ModelSerializer.DeserializeModel<Index>(data);
        }

        private IdList GetIndicesByUrlFile(long urlFileId)
        {
            var data = mDb.Get(urlFileId.GetBytes(), mUrlFileIdIndexColumnFamily);
            if (data == null)
            {
                return new IdList
                {
                    Ids = new HashSet<long>(),
                };
            }
            return ModelSerializer.DeserializeModel<IdList>(data);
        }

        private Index GetIndexCache(IndexKey indexKey)
        {
            var key = ModelSerializer.SerializeModel(indexKey);
            var data = mDb.Get(key, mIndexKeyIndexColumnFamily);
            if (data == null)
            {
                return null;
            }
            return GetIndex(data.GetLong());
        }

        private void ClearInvertedIndicesOf(long urlFileId)
        {
            using (var batch = new WriteBatch())
            {

                // Get Indices of this Url File
                var indexIdList = GetIndicesByUrlFile(urlFileId);
                foreach (var index in GetModelsByIds<Index>(indexIdList.Ids))
                {
                    // Delete it
                    batch.Delete(index.IndexId.GetBytes());

                    // Delete IndexKey index
                    var key = ModelSerializer.SerializeModel(index.Key);
                    batch.Delete(key, mIndexKeyIndexColumnFamily);
                }
                // Delete UrlFileId index
                batch.Delete(urlFileId.GetBytes(), mUrlFileIdIndexColumnFamily);

                mDb.Write(batch);
            }
        }

        public void ClearAndSaveInvertedIndices(long urlFileId, long oldUrlFileId, IList<Index> invertedIndices)
        {
            ClearInvertedIndicesOf(oldUrlFileId);

            using (var batch = new WriteBatch())
            {

                foreach (var index in invertedIndices)
                {
                    // Save Index
                    index.IndexId = GetAndUpdateCount(MetaMaxIndexId, 1) + 1;
                    var data = ModelSerializer.SerializeModel(index);
                    batch.Put(index.IndexId.GetBytes(), data);

                    // Save UrlFileId index
                    var urlFileIndex = new IdList
                    {
                        Ids = new HashSet<long> { index.IndexId },
                    };

                    batch.Merge(index.Key.UrlFileId.GetBytes(),
                        ModelSerializer.SerializeModel(urlFileIndex), mUrlFileIdIndexColumnFamily);

                    // Save IndexKey index
                    batch.Put(ModelSerializer.SerializeModel(index.Key), index.IndexId.GetBytes());
                }

                mDb.Write(batch);
            }
        }

        public Index LoadByWordInUrlFile(long urlFileId, string word)
        {
            return mIndexCache.Get(new IndexKey
            {
                UrlFileId = urlFileId,
                Word = word,
            });
        }
    }
}
