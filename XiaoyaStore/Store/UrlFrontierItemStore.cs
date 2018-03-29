using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;

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
                // Clear all
                context.RemoveRange(context.UrlFrontierItems);

                // Add all init urls
                foreach (var url in initUrls)
                {
                    var item = new UrlFrontierItem
                    {
                        Url = url,
                        PlannedTime = DateTime.Now,
                        FailedTimes = 0,
                        UpdatedAt = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        IsPopped = false,
                    };

                    context.UrlFrontierItems.Add(item);
                }

                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                { }
            }
        }

        public void RestartCrawl()
        {
            using (var context = NewContext())
            {
                // Pop all urls
                foreach (var item in context.UrlFrontierItems.Where(o => o.IsPopped))
                {
                    item.IsPopped = false;
                }

                // Don't plan too late
                foreach (var item in context.UrlFrontierItems
                    .Where(o => o.PlannedTime > DateTime.Now.AddMonths(3)))
                {
                    item.PlannedTime = DateTime.Now.AddMonths(3);
                }

                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                { }
            }
        }

        public UrlFrontierItem Push(string url)
        {
            using (var context = NewContext())
            {
                var item = context.UrlFrontierItems.SingleOrDefault(o => o.Url == url);

                if (item == null)
                {
                    // Add this url for the first time
                    item = new UrlFrontierItem
                    {
                        Url = url,
                        PlannedTime = DateTime.Now,
                        FailedTimes = 0,
                        UpdatedAt = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        IsPopped = false,
                    };

                    context.UrlFrontierItems.Add(item);

                    try
                    {
                        // Attempt to save changes to the database
                        context.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException e)
                    {
                        e.Entries.Single().Reload();
                        context.SaveChanges();
                    }
                    catch (DbUpdateException)
                    { }
                }

                return item;
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

                // Don't plan too late
                if (item.PlannedTime > DateTime.Now.AddMonths(3))
                {
                    item.PlannedTime = DateTime.Now.AddMonths(3);
                }

                try
                {
                    // Attempt to save changes to the database
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    e.Entries.Single().Reload();
                    context.SaveChanges();
                }

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

        public UrlFrontierItem PopUrlForCrawl(bool strictPlannedTime = true)
        {
            using (var context = NewContext())
            {
                var item = context.UrlFrontierItems
                .OrderBy(o => o.PlannedTime)
                .Where(o => o.IsPopped == false
                            && (!strictPlannedTime || o.PlannedTime <= DateTime.Now))
                .FirstOrDefault();

                if (item == null)
                {
                    return null;
                }

                item.IsPopped = true;
                item.UpdatedAt = DateTime.Now;

                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    e.Entries.Single().Reload();
                    context.SaveChanges();
                }
                return item;
            }
        }

        public int Count()
        {
            using (var context = NewContext())
            {
                return context.UrlFrontierItems.Count();
            }
        }
    }
}
