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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region UrlFrontierItem

            modelBuilder.Entity<UrlFrontierItem>()
                .HasIndex(o => o.Url)
                .IsUnique();

            modelBuilder.Entity<UrlFrontierItem>()
                .HasIndex(o => o.PlannedTime);

            modelBuilder.Entity<UrlFrontierItem>()
                .HasIndex(o => o.IsPopped);

            #endregion

            #region UrlFile

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => o.Url)
                .IsUnique();

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => o.FilePath)
                .IsUnique();

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => o.UpdatedAt)
                .IsUnique();

            #endregion

            #region InvertedIndex

            modelBuilder.Entity<InvertedIndex>()
               .HasIndex(o => o.Word);

            modelBuilder.Entity<InvertedIndex>()
               .HasIndex(o => new { o.UrlFileId, o.Position });

            #endregion
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlite("Data Source=Db/XiaoyaSearch.db");
        }

        public XiaoyaSearchContext() : base() { }

        public XiaoyaSearchContext(DbContextOptions options)
            : base(options) { }
    }
}
