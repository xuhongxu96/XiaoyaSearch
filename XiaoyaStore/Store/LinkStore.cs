using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Data.Model;
using System.Linq;
using XiaoyaStore.Cache;
using XiaoyaStore.Config;
using RocksDbSharp;
using XiaoyaStore.Data;
using XiaoyaStore.Data.MergeOperator;

namespace XiaoyaStore.Store
{
    public class LinkStore : CounterStore
    {
        private static readonly byte[] MetaMaxLinkId = "MaxLinkId".GetBytes();

        public override string DbFileName => "Links";

        ColumnFamilyHandle mUrlIndexColumnFamily;
        ColumnFamilyHandle mUrlFileIdIndexColumnFamily;

        protected LRUCache<string, IEnumerable<Link>> mCache;

        public LinkStore(StoreConfig config,
            bool isReadOnly = false, bool enableCache = true)
            : base(config, isReadOnly)
        {
            var columnFamilyOptions
                = new ColumnFamilyOptions().SetMergeOperator(new IdListConcatOperator());

            var columnFamilies = new ColumnFamilies
            {
                {"UrlIndex", columnFamilyOptions },
                {"UrlFileIdIndex", columnFamilyOptions },
            };

            OpenDb(columnFamilies);

            mUrlIndexColumnFamily = mDb.GetColumnFamily("UrlIndex");
            mUrlFileIdIndexColumnFamily = mDb.GetColumnFamily("UrlFileIdIndex");

            mCache = new LRUCache<string, IEnumerable<Link>>(TimeSpan.FromDays(1), GetCache, null, 30_000_000, enableCache);
        }

        private IdList GetLinksByUrl(string url)
        {
            var data = mDb.Get(url.GetBytes(), mUrlIndexColumnFamily);
            if (data == null)
            {
                return new IdList
                {
                    Ids = new HashSet<long>(),
                };
            }
            var model = ModelSerializer.DeserializeModel<IdList>(data);
            return model;
        }

        private IdList GetLinksByUrlFileId(long urlFileId)
        {
            var data = mDb.Get(urlFileId.GetBytes(), mUrlFileIdIndexColumnFamily);
            if (data == null)
            {
                return new IdList
                {
                    Ids = new HashSet<long>(),
                };
            }
            var model = ModelSerializer.DeserializeModel<IdList>(data);
            return model;
        }

        protected IEnumerable<Link> GetCache(string url)
        {
            var links = GetModelsByIds<Link>(GetLinksByUrl(url).Ids).ToHashSet();
            return links.Where(o => o != null);
        }

        private void ClearLinksForUrlFile(WriteBatch batch, long urlFileId)
        {
            var linkIds = GetLinksByUrlFileId(urlFileId);

            // For all links
            foreach (var oldLink in GetModelsByIds<Link>(linkIds.Ids))
            {
                // Delete old url index
                var newValue = new IdList
                {
                    Ids = new HashSet<long> { urlFileId },
                    IsAdd = false,
                };
                mDb.Merge(oldLink.Url.GetBytes(), ModelSerializer.SerializeModel(newValue), mUrlIndexColumnFamily);
            }

            // Delete links
            foreach (var id in linkIds.Ids)
            {
                batch.Delete(id.GetBytes());
            }

            // Delete UrlFileId index
            batch.Delete(urlFileId.GetBytes(), mUrlFileIdIndexColumnFamily);
        }

        public void SaveLinksForUrlFile(long urlFileId, long oldUrlFileId, IList<Link> links)
        {
            using (var batch = new WriteBatch())
            {

                if (oldUrlFileId != 0)
                {
                    ClearLinksForUrlFile(batch, oldUrlFileId);
                }

                // Add links and url index
                foreach (var link in links)
                {
                    // Assign new link id
                    link.LinkId = GetAndUpdateCount(MetaMaxLinkId, 1) + 1;

                    // Add new url index
                    var urlList = new IdList
                    {
                        Ids = new HashSet<long> { link.LinkId },
                    };
                    batch.Merge(link.Url.GetBytes(), ModelSerializer.SerializeModel(urlList), mUrlIndexColumnFamily);

                    // Add link
                    batch.Put(link.LinkId.GetBytes(), ModelSerializer.SerializeModel(link));
                }

                // Update UrlFileId index
                var linkIdList = new IdList
                {
                    Ids = new HashSet<long>(links.Select(o => o.LinkId)),
                };
                batch.Put(urlFileId.GetBytes(), ModelSerializer.SerializeModel(linkIdList), mUrlFileIdIndexColumnFamily);

                mDb.Write(batch);
            }
        }

        public IEnumerable<Link> LoadByUrl(string url)
        {
            return mCache.Get(url);
        }
    }
}