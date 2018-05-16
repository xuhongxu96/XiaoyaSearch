using Bond;
using Bond.IO.Safe;
using Bond.Protocols;
using RocksDbSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XiaoyaLogger;
using XiaoyaStore.Config;
using XiaoyaStore.Data;
using XiaoyaStore.Data.MergeOperator;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Helper;

namespace XiaoyaStore.Store
{
    public class UrlFrontierItemStore : BaseStore
    {
        public override string DbFileName => "UrlFrontierItems";

        ColumnFamilyHandle mHostCountColumnFamily;

        private ReaderWriterLock mReaderWriterLock = new ReaderWriterLock();

        private ConcurrentPriorityQueue<UrlFrontierItemKey, UrlFrontierItem, string> mUrlQueue =
            new ConcurrentPriorityQueue<UrlFrontierItemKey, UrlFrontierItem, string>(o => o.Url);

        private ConcurrentDictionary<string, UrlFrontierItem> mPoppedUrlDict
            = new ConcurrentDictionary<string, UrlFrontierItem>();

        private object mUrlQueueLock = new object();

        public UrlFrontierItemStore(StoreConfig config, bool isReadOnly = false)
            : base(config, isReadOnly)
        {
            var defaultOptions = new ColumnFamilyOptions();
            var options = new ColumnFamilyOptions().SetMergeOperator(new CounterOperator());
            var columnFamilies = new ColumnFamilies
            {
                { "default",  defaultOptions },
                { "HostCount", options },
            };

            OpenDb(columnFamilies);

            mHostCountColumnFamily = mDb.GetColumnFamily("HostCount");
            LoadUrlFrontierItems();
        }

        public long GetHostCount(string host)
        {
            var data = mDb.Get(host.GetBytes(), mHostCountColumnFamily);
            if (data == null)
            {
                return 0;
            }
            else
            {
                return data.GetLong();
            }
        }

        private void SaveNewUrl(UrlFrontierItem item)
        {
            using (var batch = new WriteBatch())
            {
                var data = ModelSerializer.SerializeModel(item);
                batch.Put(item.Url.GetBytes(), data);
                batch.Merge(item.Host.GetBytes(), ((long)1).GetBytes(), mHostCountColumnFamily);
                mDb.Write(batch);
            }
        }

        private void LoadUrlFrontierItems()
        {
            mConfig.Logger?.Log(nameof(UrlFrontierItemStore), "Loading UrlFrontierItems");

            using (var iter = mDb.NewIterator())
            {
                for (iter.SeekToFirst(); iter.Valid(); iter.Next())
                {
                    var data = iter.Value();

                    var item = ModelSerializer.DeserializeModel<UrlFrontierItem>(data);
                    mUrlQueue.Enqueue(item.Key, item);
                }
            }
            mConfig.Logger?.Log(nameof(UrlFrontierItemStore), "Loaded UrlFrontierItems");
        }


        public void Init(IEnumerable<string> initUrls)
        {
            // Add all init urls
            foreach (var url in initUrls)
            {
                if (mUrlQueue.ContainsValue(url))
                {
                    // Already exists, skip
                    continue;
                }

                var item = new UrlFrontierItem
                {
                    Url = url,
                    PlannedTime = DateTime.Now.ToUniversalTime(),
                    FailedTimes = 0,
                    UpdatedAt = DateTime.Now.ToUniversalTime(),
                    CreatedAt = DateTime.Now.ToUniversalTime(),
                };

                SaveNewUrl(item);

                mUrlQueue.Enqueue(item.Key, item);
            }
        }

        public void PushUrls(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                try
                {
                    mReaderWriterLock.AcquireReaderLock(1000);

                    if (mUrlQueue.ContainsValue(url)
                    || mPoppedUrlDict.ContainsKey(url))
                    {
                        // Already exists or is popped, skip
                        continue;
                    }
                }
                finally
                {
                    mReaderWriterLock.ReleaseReaderLock();
                }

                var item = new UrlFrontierItem
                {
                    Url = url,
                    PlannedTime = DateTime.Now.ToUniversalTime(),
                    FailedTimes = 0,
                    UpdatedAt = DateTime.Now.ToUniversalTime(),
                    CreatedAt = DateTime.Now.ToUniversalTime(),
                };

                var urlDepth = UrlHelper.GetDomainDepth(url);

                // Update host stats
                var hostCount = GetHostCount(item.Host);

                // Calculate priority
                item.Priority = hostCount + urlDepth * 10;

                SaveNewUrl(item);

                mUrlQueue.Enqueue(item.Key, item);
            }
        }

        public bool PushBack(string url, TimeSpan updateInterval, bool failed = false)
        {
            try
            {
                mReaderWriterLock.AcquireReaderLock(1000);

                if (!mPoppedUrlDict.ContainsKey(url))
                {
                    // Not popped, skip
                    return false;
                }
            }
            finally
            {
                mReaderWriterLock.ReleaseReaderLock();
            }

            if (!mPoppedUrlDict.TryRemove(url, out var item))
            {
                // Fail to push back
                return false;
            }

            if (failed)
            {
                // Failed to fetch last time
                item.FailedTimes++;
                item.PlannedTime = DateTime.Now.ToUniversalTime().AddDays(item.FailedTimes);
                item.UpdatedAt = DateTime.Now.ToUniversalTime();
            }
            else
            {
                // Add this url again
                item.FailedTimes = 0;
                item.PlannedTime = DateTime.Now.ToUniversalTime().Add(updateInterval);
                item.UpdatedAt = DateTime.Now.ToUniversalTime();
            }

            var urlDepth = UrlHelper.GetDomainDepth(url);

            item.Priority = GetHostCount(item.Host);
            item.Priority += urlDepth * 10;

            // Persist data
            var data = ModelSerializer.SerializeModel(item);
            mDb.Put(url.GetBytes(), data);

            mUrlQueue.Enqueue(item.Key, item);

            return true;
        }

        public string PopUrlForCrawl()
        {
            try
            {
                mReaderWriterLock.AcquireWriterLock(1000);

                if (mUrlQueue.TryDequeue(out var result)
                && mPoppedUrlDict.TryAdd(result.Value.Url, result.Value))
                {
                    return result.Value.Url;
                }

                return null;
            }
            finally
            {
                mReaderWriterLock.ReleaseWriterLock();
            }
        }

        public void Remove(string url)
        {
            using (var batch = new WriteBatch())
            {
                batch.Delete(url.GetBytes());
                batch.Merge(UrlHelper.GetHost(url).GetBytes(), ((long)-1).GetBytes(), mHostCountColumnFamily);
                mDb.Write(batch);
            }
        }
    }

}