using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Data
{
    public class XiaoyaSearchContext : DbContext
    {
        public DbSet<UrlFrontierItem> UrlFrontierItems { get; set; }
        public DbSet<UrlFile> UrlFiles { get; set; }
        public DbSet<InvertedIndex> InvertedIndices { get; set; }
        public DbSet<IndexStat> IndexStats { get; set; }
        public DbSet<UrlFileIndexStat> UrlFileIndexStats { get; set; }
        public DbSet<UrlHostStat> UrlHostStats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region UrlFrontierItem

            modelBuilder.Entity<UrlFrontierItem>()
                .HasIndex(o => o.Url)
                .IsUnique();

            modelBuilder.Entity<UrlFrontierItem>()
                .HasIndex(o => new { o.PlannedTime, o.IsPopped });

            #endregion

            #region UrlFile

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => o.Url)
                .IsUnique();

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => o.FilePath)
                .IsUnique();

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => new { o.UpdatedAt, o.IndexStatus });

            #endregion

            #region InvertedIndex

            modelBuilder.Entity<InvertedIndex>()
               .HasIndex(o => new { o.UrlFileId, o.Word, o.Position, o.IndexType })
               .IsUnique();

            #endregion

            #region IndexStat

            modelBuilder.Entity<IndexStat>()
                .HasIndex(o => o.Word)
                .IsUnique();

            #endregion

            #region UrlFileIndexStat

            modelBuilder.Entity<UrlFileIndexStat>()
                .HasIndex(o => new { o.Word, o.UrlFileId })
                .IsUnique();

            modelBuilder.Entity<UrlFileIndexStat>()
                .HasIndex(o => o.WordFrequency);

            #endregion

            #region UrlHostStat

            modelBuilder.Entity<UrlHostStat>()
                .HasIndex(o => o.Host)
                .IsUnique();

            #endregion
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=Db/XiaoyaSearch.db");
            }
        }

        public XiaoyaSearchContext() : base() { }

        public XiaoyaSearchContext(DbContextOptions options)
            : base(options) { }
    }
}
