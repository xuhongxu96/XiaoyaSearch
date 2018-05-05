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
        public DbSet<UrlHostStat> UrlHostStats { get; set; }
        public DbSet<Link> Links { get; set; }
        public DbSet<SameUrl> SameUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region UrlFrontierItem

            modelBuilder.Entity<UrlFrontierItem>()
                .HasIndex(o => o.Url)
                .IsUnique();

            modelBuilder.Entity<UrlFrontierItem>()
                .HasIndex(o => new { o.IsPopped, o.PlannedTime, o.Priority });

            modelBuilder.Entity<UrlFrontierItem>()
                .HasIndex(o => o.IsPopped);

            modelBuilder.Entity<UrlFrontierItem>()
                .HasIndex(o => o.Host);

            #endregion

            #region UrlFile

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => o.Url)
                .IsUnique();

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => o.FilePath)
                .IsUnique();

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => o.FileHash);

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => new { o.IndexStatus, o.UpdatedAt });

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => o.IndexStatus);

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => o.PublishDate);

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => new { o.UrlFileId, o.PageRank });

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => o.PageRank);

            #endregion

            #region InvertedIndex

            modelBuilder.Entity<InvertedIndex>()
               .HasIndex(o => new { o.Word, o.UrlFileId })
               .IsUnique();

            modelBuilder.Entity<InvertedIndex>()
              .HasIndex(o => new { o.Word, o.Weight });

            modelBuilder.Entity<InvertedIndex>()
              .HasIndex(o => new { o.Word });

            modelBuilder.Entity<InvertedIndex>()
              .HasIndex(o => new { o.Weight });

            modelBuilder.Entity<InvertedIndex>()
              .HasIndex(o => new { o.UrlFileId });

            modelBuilder.Entity<InvertedIndex>()
                .Ignore(o => o.PositionArr);

            #endregion

            #region IndexStat

            modelBuilder.Entity<IndexStat>()
                .HasIndex(o => o.Word)
                .IsUnique();

            modelBuilder.Entity<IndexStat>()
               .HasIndex(o => o.WordFrequency);

            #endregion

            #region UrlHostStat

            modelBuilder.Entity<UrlHostStat>()
                            .HasIndex(o => o.Host)
                            .IsUnique();

            #endregion

            #region Link

            modelBuilder.Entity<Link>()
                .HasIndex(o => o.Url);

            modelBuilder.Entity<Link>()
                .HasIndex(o => o.UrlFileId);

            #endregion

            #region SameUrl

            modelBuilder.Entity<SameUrl>()
                .HasIndex(o => o.RawUrl)
                .IsUnique();

            modelBuilder.Entity<SameUrl>()
                .HasIndex(o => o.Url);

            #endregion
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=IR-PC;Initial Catalog=XiaoyaSearch;Integrated Security=True");
            }
        }

        public XiaoyaSearchContext() : base() { }

        public XiaoyaSearchContext(DbContextOptions options)
            : base(options) { }
    }
}
