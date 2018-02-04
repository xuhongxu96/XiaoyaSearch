using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaCommon.Data.Crawler.Model;

namespace XiaoyaCommon.Data.Crawler
{
    public class CrawlerContext: DbContext
    {
        public DbSet<UrlFile> UrlFiles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Db/Crawler.db");
        }
    }
}
