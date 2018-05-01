using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Helper;
using Z.EntityFramework.Plus;

namespace XiaoyaStore.Store
{
    public class UrlFrontierItemStore : BaseStore, IUrlFrontierItemStore
    {
        public UrlFrontierItemStore(DbContextOptions options = null) : base(options)
        { }

        public void Init(IEnumerable<string> initUrls)
        {
            using (var context = NewContext())
            {
                var hostCount = new Dictionary<string, int>();

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
                        if (hostCount.ContainsKey(host))
                        {
                            hostCount[host]++;
                        }
                        else
                        {
                            hostCount[host] = 1;
                        }
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

                foreach (var host in hostCount)
                {
                    var hostStat = context.UrlHostStats.SingleOrDefault(o => o.Host == host.Key);
                    if (hostStat == null)
                    {
                        context.UrlHostStats.Add(new UrlHostStat
                        {
                            Host = host.Key,
                            Count = host.Value,
                        });
                    }
                    else
                    {
                        hostStat.Count += host.Value;
                    }
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
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                var existedUrlSet = context.UrlFrontierItems
                    .Where(o => urls.Contains(o.Url))
                    .Select(o => o.Url);

                var newUrls = urls.Except(existedUrlSet).ToList();

                var hostStats = newUrls.GroupBy(o => UrlHelper.GetHost(o))
                    .Select(o => new UrlHostStat
                    {
                        Host = o.Key,
                        Count = o.Count()
                    });

                var existedHostStats = context.UrlHostStats
                    .Where(o => hostStats.Select(p => p.Host).Contains(o.Host))
                    .ToDictionary(o => o.Host);

                foreach (var host in hostStats)
                {
                    if (existedHostStats.ContainsKey(host.Host))
                    {
                        existedHostStats[host.Host].Count += host.Count;
                    }
                    else
                    {
                        existedHostStats[host.Host] = host;
                    }
                }

                context.BulkInsertOrUpdate(existedHostStats.Values.ToList());

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

                    if (existedHostStats.ContainsKey(host))
                    {
                        item.PlannedTime = item.PlannedTime.AddSeconds(existedHostStats[host].Count * new Random().NextDouble() * 30);
                    }

                    // Don't plan too late
                    if (item.PlannedTime > DateTime.Now.AddDays(3))
                    {
                        item.PlannedTime = DateTime.Now.AddDays(3);
                    }

                    urlList.Add(item);
                }
                context.BulkInsert(urlList);
                context.SaveChanges();
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

                var hostStat = context.UrlHostStats.SingleOrDefault(o => o.Host == UrlHelper.GetHost(url));
                if (hostStat != null)
                {
                    item.PlannedTime = item.PlannedTime.AddSeconds((hostStat.Count - 1) * 10);
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
                    var host = context.UrlHostStats.SingleOrDefault(o => o.Host == item.Host);
                    if (host != null)
                    {
                        host.Count--;
                    }
                    context.Remove(item);
                }
                context.SaveChanges();
            }
        }
    }
}
