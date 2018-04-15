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
        public DbSet<Link> Links { get; set; }

        /*
         * Additional Migration
         * 
      migrationBuilder.Sql(@"CREATE VIEW dbo.IndexStats WITH SCHEMABINDING AS 
SELECT 
        Word,
        COUNT_BIG(*) AS DocumentFrequency,
		SUM(WordFrequency) AS WordFrequency
FROM dbo.UrlFileIndexStats
GROUP BY Word");
        *
        * 
        * 
            ALTER DATABASE DBNAME COLLATE Chinese_PRC_90_CI_AS_SC
        * 
        */

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region UrlFrontierItem

            modelBuilder.Entity<UrlFrontierItem>()
                .HasIndex(o => o.Url)
                .IsUnique();

            modelBuilder.Entity<UrlFrontierItem>()
                .HasIndex(o => new { o.PlannedTime, o.IsPopped });

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
                .HasIndex(o => new { o.UpdatedAt, o.IndexStatus });

            modelBuilder.Entity<UrlFile>()
                .HasIndex(o => o.IndexStatus);

            #endregion

            #region InvertedIndex

            modelBuilder.Entity<InvertedIndex>()
               .HasIndex(o => new { o.UrlFileId, o.Word, o.Position, o.IndexType })
               .IsUnique();

            modelBuilder.Entity<InvertedIndex>()
               .HasIndex(o => o.UrlFileId);

            #endregion

            #region IndexStat

            if (Database.IsSqlServer())
            {
                // modelBuilder.Ignore<IndexStat>();

                //*
                modelBuilder.Entity<IndexStat>()
                    .Ignore(o => o.IndexStatId);

                modelBuilder.Entity<IndexStat>()
                            .HasKey(o => o.Word);
                // */
            }
            else
            {
                modelBuilder.Entity<IndexStat>()
                    .HasIndex(o => o.Word)
                    .IsUnique();
            }
            #endregion

            #region UrlFileIndexStat

            modelBuilder.Entity<UrlFileIndexStat>()
                            .HasIndex(o => new { o.Word, o.UrlFileId })
                            .IsUnique();

            modelBuilder.Entity<UrlFileIndexStat>()
                            .HasIndex(o => new { o.Word, o.UrlFileId, o.Weight, o.WordFrequency });

            modelBuilder.Entity<UrlFileIndexStat>()
                .HasIndex(o => o.WordFrequency);

            modelBuilder.Entity<UrlFileIndexStat>()
                .HasIndex(o => o.Word);

            modelBuilder.Entity<UrlFileIndexStat>()
                .HasIndex(o => o.Weight);

            modelBuilder.Entity<UrlFileIndexStat>()
                .HasIndex(o => o.UrlFileId);

            #endregion

            #region UrlHostStat

            modelBuilder.Entity<UrlHostStat>()
                            .HasIndex(o => o.Host)
                            .IsUnique();

            #endregion

            #region Link

            modelBuilder.Entity<Link>()
                .HasIndex(o => o.Url);

            #endregion
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // optionsBuilder.UseSqlite("Data Source=Db/XiaoyaSearch.db");
                optionsBuilder.UseSqlServer("Data Source=IR-PC;Initial Catalog=XiaoyaSearch;Integrated Security=True");
            }
        }

        public XiaoyaSearchContext() : base() { }

        public XiaoyaSearchContext(DbContextOptions options)
            : base(options) { }
    }
}
