using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XiaoyaLogger;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Helper;
using Z.EntityFramework.Plus;

namespace XiaoyaStore.Store
{
    public class UrlFrontierItemStore : BaseStore, IUrlFrontierItemStore
    {
        private ConcurrentDictionary<string, int> mHostStat = new ConcurrentDictionary<string, int>();
        private ConcurrentQueue<string> mUrlQueue = new ConcurrentQueue<string>();

        private RuntimeLogger mLogger = null;
        private object mUrlQueueLock = new object();

        public UrlFrontierItemStore(DbContextOptions options = null, RuntimeLogger logger = null) : base(options)
        {
            using (var context = NewContext())
            {
                var hostStats = context.UrlFrontierItems
                    .GroupBy(o => o.Host)
                    .Select(o => new UrlHostStat
                    {
                        Host = o.Key,
                        Count = o.Count(),
                    });
                foreach (var stat in hostStats)
                {
                    if (stat.Host != null)
                    {
                        mHostStat.TryAdd(stat.Host, stat.Count);
                    }
                }
            }

            mLogger = logger;

            
            var thread = new Thread(FlushHostStat)
            {
                Priority = ThreadPriority.AboveNormal,
            };
            thread.Start();
            
        }

        
        private void FlushHostStat()
        {
            while (true)
            {
                Thread.Sleep(10000);

                mLogger?.Log(nameof(UrlFrontierItemStore), "Flushing Host Stats");

                using (var context = NewContext())
                {
                    var hostStats = new List<UrlHostStat>();
                    foreach (var item in mHostStat)
                    {
                        hostStats.Add(new UrlHostStat
                        {
                            Host = item.Key,
                            Count = item.Value,
                        });
                    }
                    context.UrlHostStats.Delete();
                    if (context.Database.IsSqlServer())
                    {
                        context.BulkInsert(hostStats);
                    }
                    else
                    {
                        context.AddRange(hostStats);
                    }
                    context.SaveChanges();
                }

                mLogger?.Log(nameof(UrlFrontierItemStore), "Flushed Host Stats");
            }
        }
        

        public void Init(IEnumerable<string> initUrls)
        {
            using (var context = NewContext())
            {
                // Add all init urls
                foreach (var url in initUrls)
                {
                    if (context.UrlFrontierItems.Any(o => o.Url == url))
                    {
                        continue;
                    }
                    var host = UrlHelper.GetHost(url);

                    if (host != "")
                    {
                        mHostStat.AddOrUpdate(host, 1, (k, v) => v + 1);
                    }

                    var item = new UrlFrontierItem
                    {
                        Url = url,
                        Host = host,
                        UrlDepth = UrlHelper.GetDomainDepth(url),
                        PlannedTime = DateTime.Now,
                        FailedTimes = 0,
                        UpdatedAt = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        IsPopped = false,
                    };

                    context.UrlFrontierItems.Add(item);
                }

                context.SaveChanges();
            }
        }

        public void RestartCrawl()
        {
            using (var context = NewContext())
            {
                // Pop all urls
                context.Database.ExecuteSqlCommand("UPDATE UrlFrontierItems SET IsPopped = 0 WHERE IsPopped = 1");

                context.SaveChanges();
            }
        }

        public void PushUrls(IEnumerable<string> urls)
        {
            using (var context = NewContext())
            {
#if DEBUG
                var time = DateTime.Now;
                Console.WriteLine("Pushing Urls: " + "\n" + (DateTime.Now - time).TotalSeconds);
#endif
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                var existedUrlSet = context.UrlFrontierItems
                    .Where(o => urls.Contains(o.Url))
                    .Select(o => o.Url);

                var newUrls = urls.Except(existedUrlSet).ToList();

#if DEBUG
                Console.WriteLine("Got New Urls: " + "\n" + (DateTime.Now - time).TotalSeconds);
                time = DateTime.Now;
#endif
                var urlList = new List<UrlFrontierItem>();

                foreach (var url in newUrls)
                {
                    // Add this url if not exists yet
                    var host = UrlHelper.GetHost(url);
                    var item = new UrlFrontierItem
                    {
                        Url = url,
                        Host = host,
                        UrlDepth = UrlHelper.GetDomainDepth(url),
                        PlannedTime = DateTime.Now,
                        FailedTimes = 0,
                        UpdatedAt = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        IsPopped = false,
                    };

                    mHostStat.AddOrUpdate(host, 1, (k, v) => v + 1);

                    item.Priority = mHostStat[host] + item.UrlDepth * 10;

                    urlList.Add(item);
                }
                if (context.Database.IsSqlServer())
                {
                    context.BulkInsert(urlList);
                }
                else
                {
                    context.AddRange(urlList);
                }
#if DEBUG
                Console.WriteLine("Inserted new urls: " + "\n" + (DateTime.Now - time).TotalSeconds);
                time = DateTime.Now;
#endif
                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateException)
                { }
            }
        }

        public UrlFrontierItem PushBack(string url, bool failed = false)
        {
            using (var context = NewContext())
            {
                var item = context.UrlFrontierItems.SingleOrDefault(o => o.Url == url);

                if (item == null || !item.IsPopped)
                {
                    // If no item or not popped but added again, skip
                    return item;
                }

                var urlFile = context.UrlFiles.SingleOrDefault(o => o.Url == url);
                if (failed || urlFile == null)
                {
                    // Failed to fetch last time
                    item.FailedTimes++;
                    item.PlannedTime = DateTime.Now.AddDays(item.FailedTimes);
                    item.IsPopped = false;
                    item.UpdatedAt = DateTime.Now;
                }
                else
                {
                    // Add this url again
                    item.FailedTimes = 0;
                    item.PlannedTime = DateTime.Now.Add(urlFile.UpdateInterval);
                    item.IsPopped = false;
                    item.UpdatedAt = DateTime.Now;
                }

                var host = UrlHelper.GetHost(url);
                if (mHostStat.ContainsKey(host))
                {
                    item.Priority = mHostStat[host];
                }

                item.Priority += item.UrlDepth * 10;

                // Attempt to save changes to the database
                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateException)
                { }

                return item;
            }
        }

        public UrlFrontierItem LoadByUrl(string url)
        {
            using (var context = NewContext())
            {
                return context.UrlFrontierItems.SingleOrDefault(o => o.Url == url);
            }
        }

        public string PopUrlForCrawl()
        {
            if (mUrlQueue.TryDequeue(out string url))
            {
                return url;
            }

            lock (mUrlQueueLock)
            {
                if (mUrlQueue.TryDequeue(out url))
                {
                    return url;
                }

                using (var context = NewContext())
                {
                    IEnumerable<string> urls;
                    if (context.Database.IsSqlServer())
                    {
                        urls = context.UrlFrontierItems
                            .FromSql("SELECT TOP 100 * FROM UrlFrontierItems WHERE IsPopped = 0 AND PlannedTime <= GETDATE() ORDER BY Priority, PlannedTime")
                            .Select(o => o.Url)
                            .Take(100)
                            .ToList();

                    }
                    else
                    {
                        var now = DateTime.Now;
                        urls = context.UrlFrontierItems
                            .Where(o => !o.IsPopped && o.PlannedTime <= now)
                            .OrderBy(o => o.Priority)
                            .ThenBy(o => o.PlannedTime)
                            .Select(o => o.Url)
                            .Take(100)
                            .ToList();
                    }

                    context.UrlFrontierItems
                        .Where(o => urls.Contains(o.Url))
                        .Update(o => new UrlFrontierItem
                        {
                            IsPopped = true,
                            UpdatedAt = DateTime.Now,
                        });

                    try
                    {
                        context.SaveChanges();
                    }
                    catch (DbUpdateException)
                    {
                        return null;
                    }

                    foreach (var item in urls)
                    {
                        mUrlQueue.Enqueue(item);
                    }
                }
            }

            if (mUrlQueue.TryDequeue(out url))
            {
                return url;
            }

            return null;
        }

        public void Remove(string url)
        {
            using (var context = NewContext())
            {
                var item = context.UrlFrontierItems.SingleOrDefault(o => o.Url == url);
                if (item != null)
                {
                    mHostStat.AddOrUpdate(UrlHelper.GetHost(url), 0, (k, v) => v - 1);
                    context.Remove(item);
                }
                context.SaveChanges();
            }
        }
    }
}
