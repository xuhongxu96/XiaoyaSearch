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

        private RuntimeLogger mLogger = null;

        public UrlFrontierItemStore(DbContextOptions options = null, RuntimeLogger logger = null) : base(options)
        {
            using (var context = NewContext())
            {
                foreach (var stat in context.UrlHostStats)
                {
                    mHostStat.TryAdd(stat.Host, stat.Count);
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
                    context.BulkInsert(hostStats);
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

                    item.PlannedTime = item.PlannedTime.AddHours(item.UrlDepth);

                    mHostStat.AddOrUpdate(host, 1, (k, v) => v + 1);

                    item.PlannedTime = item.PlannedTime.AddSeconds(mHostStat[host] * new Random().NextDouble() * 30.0);

                    // Don't plan too late
                    if (item.PlannedTime > DateTime.Now.AddDays(3))
                    {
                        item.PlannedTime = DateTime.Now.AddDays(3);
                    }

                    urlList.Add(item);
                }
                context.BulkInsert(urlList);
#if DEBUG
                Console.WriteLine("Inserted new urls: " + "\n" + (DateTime.Now - time).TotalSeconds); 
                time = DateTime.Now;
#endif
                try
                {
                    context.SaveChanges();
                }
                catch (SqlException e)
                {
                    var t = e;
                }
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
                    item.PlannedTime = DateTime.Now.AddDays(item.FailedTimes * 2);
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
                    item.PlannedTime = item.PlannedTime.AddSeconds((mHostStat[host] - 1) * 10);
                }

                item.PlannedTime.AddHours(item.UrlDepth);

                // Don't plan too late
                if (item.PlannedTime > DateTime.Now.AddMonths(3))
                {
                    item.PlannedTime = DateTime.Now.AddMonths(3);
                }

                // Refresh the first 3000 urls
                if (item.UrlFrontierItemId < 3000
                    && (item.UpdatedAt - item.CreatedAt).TotalHours < 3
                    && item.PlannedTime > DateTime.Now.AddHours(5))
                {
                    item.PlannedTime = DateTime.Now.AddHours(5);
                }

                // Attempt to save changes to the database
                context.SaveChanges();

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

        public UrlFrontierItem PopUrlForCrawl()
        {
            using (var context = NewContext())
            {
                UrlFrontierItem item;
                if (context.Database.IsSqlServer())
                {
                    item = context.UrlFrontierItems
                        .FromSql("SELECT TOP 1 * FROM UrlFrontierItems WHERE IsPopped = 0 ORDER BY PlannedTime")
                        .FirstOrDefault();
                }
                else
                {
                    item = context.UrlFrontierItems
                        .Where(o => !o.IsPopped)
                        .OrderBy(o => o.PlannedTime)
                        .FirstOrDefault();
                }
                if (item == null)
                {
                    return null;
                }

                item.IsPopped = true;
                item.UpdatedAt = DateTime.Now;

                context.SaveChanges();

                return item;
            }
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
