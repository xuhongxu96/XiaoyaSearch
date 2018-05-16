using Bond;
using Bond.IO.Safe;
using Bond.Protocols;
using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Cache;
using XiaoyaStore.Config;
using XiaoyaStore.Data;
using XiaoyaStore.Data.MergeOperator;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Helper;


namespace XiaoyaStore.Store
{
    public class UrlFileStore : CounterStore
    {
        private static readonly byte[] MetaMaxUrlFileId = "MaxUrlFileId".GetBytes();

        public override string DbFileName => "UrlFiles";

        ColumnFamilyHandle mUrlFileIndexQueueColumnFamily;
        ColumnFamilyHandle mUrlIndexColumnFamily;
        ColumnFamilyHandle mHashIndexColumnFamily;

        Iterator mIndexQueueIter = null;

        private LRUCache<long, UrlFile> mCache;

        public UrlFileStore(StoreConfig config,
            bool isReadOnly = false,
            bool enableCache = true) : base(config, isReadOnly)
        {
            var columnFamilyOptions
            = new ColumnFamilyOptions().SetMergeOperator(new IdListConcatOperator());

            var columnFamilies = new ColumnFamilies
            {
                {"UrlFileIndexQueue", columnFamilyOptions },
                {"UrlIndex", columnFamilyOptions },
                {"HashIndex", columnFamilyOptions },
            };

            OpenDb(columnFamilies);

            mUrlFileIndexQueueColumnFamily = mDb.GetColumnFamily("UrlFileIndexQueue");
            mUrlIndexColumnFamily = mDb.GetColumnFamily("UrlIndex");
            mHashIndexColumnFamily = mDb.GetColumnFamily("HashIndex");

            mCache = new LRUCache<long, UrlFile>(TimeSpan.FromDays(1), GetUrlFile, null, 500_000, enableCache);
        }

        private UrlFile GetUrlFile(long id)
        {
            var key = id.GetBytes();

            var data = mDb.Get(key);
            if (data == null)
            {
                return null;
            }

            return ModelSerializer.DeserializeModel<UrlFile>(data);
        }

        protected IEnumerable<Tuple<long, UrlFile>> LoadCachesOfUrlFiles(IEnumerable<long> urlFileIds)
        {
            var urlFileIdSet = new HashSet<long>(urlFileIds);

            foreach (var item in GetModelsByIds<UrlFile>(urlFileIds))
            {
                if (mCache.IsValid(item.UrlFileId))
                {
                    continue;
                }

                yield return Tuple.Create(item.UrlFileId, item);
            }
        }

        public long Count()
        {
            return GetMaxUrlFileId();
        }

        public UrlFile LoadAnyForIndex()
        {
            if (mIndexQueueIter == null || !mIndexQueueIter.Valid())
            {
                if (mIndexQueueIter != null)
                {
                    mIndexQueueIter.Dispose();
                }
                mIndexQueueIter = mDb.NewIterator(mUrlFileIndexQueueColumnFamily);
                mIndexQueueIter.SeekToFirst();
                if (!mIndexQueueIter.Valid())
                {
                    return null;
                }
            }

            var urlFileId = BitConverter.ToInt64(mIndexQueueIter.Value(), 0);
            return GetUrlFile(urlFileId);
        }

        public void FinishIndex(int urlFileId)
        {
            mDb.Remove(urlFileId.GetBytes(), mUrlFileIndexQueueColumnFamily);
        }

        private long GetMaxUrlFileId()
        {
            return GetCount(MetaMaxUrlFileId);
        }

        private IdList GetUrlFileByHash(string hash)
        {
            var data = mDb.Get(hash.GetBytes(), mHashIndexColumnFamily);
            if (data == null)
            {
                return null;
            }
            else
            {
                var model = ModelSerializer.DeserializeModel<IdList>(data);
                return model;
            }
        }

        public UrlFile LoadById(long id)
        {
            return mCache.Get(id);
        }

        public UrlFile LoadByUrl(string url)
        {
            var data = mDb.Get(url, mUrlIndexColumnFamily);
            if (data == null)
            {
                return null;
            }

            var urlFileId = long.Parse(data);

            return LoadById(urlFileId);
        }

        public UrlFile Save(UrlFile urlFile, out long oldUrlFileId)
        {
            // Find if the url exists
            var oldUrlFile = LoadByUrl(urlFile.Url);
            var addToIndex = false;
            using (var batch = new WriteBatch())
            {

                if (oldUrlFile == null)
                {
                    oldUrlFileId = 0;
                    // first see this url, add to database
                    urlFile.UpdatedAt = DateTime.Now;
                    urlFile.CreatedAt = DateTime.Now;
                    urlFile.UpdateInterval = TimeSpan.FromDays(1);

                    addToIndex = true;
                }
                else
                {
                    oldUrlFileId = oldUrlFile.UrlFileId;

                    // Exists this url, then judge if two fetched file is same
                    if (oldUrlFile.Title != urlFile.Title
                            || oldUrlFile.Content != urlFile.Content)
                    {
                        // Updated
                        oldUrlFile.UpdatedAt = DateTime.Now;
                        addToIndex = true;
                    }

                    // Update UpdateInterval
                    var updateInterval = DateTime.Now.Subtract(oldUrlFile.UpdatedAt);
                    oldUrlFile.UpdateInterval
                        = (oldUrlFile.UpdateInterval * 3 + updateInterval) / 4;

                    // Remove old hash index if changed
                    if (oldUrlFile.FileHash != urlFile.FileHash)
                    {
                        var removingHashIndex = new IdList
                        {
                            Ids = new HashSet<long> { oldUrlFileId },
                            IsAdd = false,
                        };
                        batch.Merge(oldUrlFile.FileHash.GetBytes(),
                            ModelSerializer.SerializeModel(removingHashIndex), mHashIndexColumnFamily);

                        oldUrlFile.FileHash = urlFile.FileHash;
                    }

                    // Update other info
                    oldUrlFile.FilePath = urlFile.FilePath;
                    oldUrlFile.Title = urlFile.Title;
                    oldUrlFile.TextContent = urlFile.TextContent;
                    oldUrlFile.PublishDate = urlFile.PublishDate;
                    oldUrlFile.Charset = urlFile.Charset;
                    oldUrlFile.MimeType = urlFile.MimeType;
                    oldUrlFile.HeaderCount = urlFile.HeaderCount;
                    oldUrlFile.HeaderTotalLength = urlFile.HeaderTotalLength;
                    oldUrlFile.InLinkCount = urlFile.InLinkCount;
                    oldUrlFile.InLinkTotalLength = urlFile.InLinkTotalLength;

                    // Remove old UrlFile
                    batch.Delete(oldUrlFileId.GetBytes());

                    urlFile = oldUrlFile;
                }

                // Assign new id
                var id = urlFile.UrlFileId = GetAndUpdateCount(MetaMaxUrlFileId, 1) + 1;

                // Overwrite url index
                batch.Put(urlFile.Url.GetBytes(), id.GetBytes(), mUrlIndexColumnFamily);

                // Add new hash index
                var deltaHashIndex = new IdList
                {
                    Ids = new HashSet<long> { id },
                };
                batch.Merge(urlFile.FileHash.GetBytes(), ModelSerializer.SerializeModel(deltaHashIndex), mHashIndexColumnFamily);

                // Add new UrlFile
                batch.Put(id.GetBytes(), ModelSerializer.SerializeModel(urlFile));

                if (addToIndex)
                {
                    batch.Put(id.GetBytes(), id.GetBytes(), mUrlFileIndexQueueColumnFamily);
                }

                mDb.Write(batch);
            }

            return urlFile;
        }

        public IEnumerable<UrlFile> LoadByHash(string hash)
        {
            return GetModelsByIds<UrlFile>(GetUrlFileByHash(hash).Ids);
        }
    }
}
