﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Migrations
{
    [DbContext(typeof(XiaoyaSearchContext))]
    [Migration("20180415081631_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("XiaoyaStore.Data.Model.InvertedIndex", b =>
                {
                    b.Property<int>("InvertedIndexId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("IndexType")
                        .IsConcurrencyToken();

                    b.Property<int>("Position")
                        .IsConcurrencyToken();

                    b.Property<int>("UrlFileId")
                        .IsConcurrencyToken();

                    b.Property<string>("Word")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("InvertedIndexId");

                    b.HasIndex("UrlFileId");

                    b.HasIndex("UrlFileId", "Word", "Position", "IndexType")
                        .IsUnique()
                        .HasFilter("[Word] IS NOT NULL");

                    b.ToTable("InvertedIndices");
                });

            modelBuilder.Entity("XiaoyaStore.Data.Model.Link", b =>
                {
                    b.Property<int>("LinkId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Text");

                    b.Property<string>("Url");

                    b.Property<int>("UrlFileId");

                    b.HasKey("LinkId");

                    b.HasIndex("Url");

                    b.ToTable("Links");
                });

            modelBuilder.Entity("XiaoyaStore.Data.Model.UrlFile", b =>
                {
                    b.Property<int>("UrlFileId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Charset")
                        .IsConcurrencyToken();

                    b.Property<string>("Content")
                        .IsConcurrencyToken();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("FileHash")
                        .IsConcurrencyToken();

                    b.Property<string>("FilePath")
                        .IsConcurrencyToken();

                    b.Property<int>("IndexStatus")
                        .IsConcurrencyToken();

                    b.Property<string>("MimeType")
                        .IsConcurrencyToken();

                    b.Property<double>("PageRank");

                    b.Property<string>("Title");

                    b.Property<double>("UpdateIntervalSeconds")
                        .IsConcurrencyToken();

                    b.Property<DateTime>("UpdatedAt")
                        .IsConcurrencyToken();

                    b.Property<string>("Url");

                    b.HasKey("UrlFileId");

                    b.HasIndex("FilePath")
                        .IsUnique()
                        .HasFilter("[FilePath] IS NOT NULL");

                    b.HasIndex("IndexStatus");

                    b.HasIndex("Url")
                        .IsUnique()
                        .HasFilter("[Url] IS NOT NULL");

                    b.HasIndex("UpdatedAt", "IndexStatus");

                    b.ToTable("UrlFiles");
                });

            modelBuilder.Entity("XiaoyaStore.Data.Model.UrlFileIndexStat", b =>
                {
                    b.Property<int>("UrlFileIndexStatId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("UrlFileId")
                        .IsConcurrencyToken();

                    b.Property<double>("Weight");

                    b.Property<string>("Word")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(30)");

                    b.Property<long>("WordFrequency")
                        .IsConcurrencyToken();

                    b.HasKey("UrlFileIndexStatId");

                    b.HasIndex("UrlFileId");

                    b.HasIndex("Weight");

                    b.HasIndex("Word");

                    b.HasIndex("WordFrequency");

                    b.HasIndex("Word", "UrlFileId")
                        .IsUnique()
                        .HasFilter("[Word] IS NOT NULL");

                    b.HasIndex("Word", "UrlFileId", "Weight", "WordFrequency");

                    b.ToTable("UrlFileIndexStats");
                });

            modelBuilder.Entity("XiaoyaStore.Data.Model.UrlFrontierItem", b =>
                {
                    b.Property<int>("UrlFrontierItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<int>("FailedTimes")
                        .IsConcurrencyToken();

                    b.Property<string>("Host")
                        .IsConcurrencyToken();

                    b.Property<bool>("IsPopped")
                        .IsConcurrencyToken();

                    b.Property<DateTime>("PlannedTime")
                        .IsConcurrencyToken();

                    b.Property<DateTime>("UpdatedAt")
                        .IsConcurrencyToken();

                    b.Property<string>("Url")
                        .IsConcurrencyToken();

                    b.Property<int>("UrlDepth")
                        .IsConcurrencyToken();

                    b.HasKey("UrlFrontierItemId");

                    b.HasIndex("IsPopped");

                    b.HasIndex("Url")
                        .IsUnique()
                        .HasFilter("[Url] IS NOT NULL");

                    b.HasIndex("PlannedTime", "IsPopped");

                    b.ToTable("UrlFrontierItems");
                });

            modelBuilder.Entity("XiaoyaStore.Data.Model.UrlHostStat", b =>
                {
                    b.Property<int>("UrlHostStatId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Count");

                    b.Property<string>("Host");

                    b.HasKey("UrlHostStatId");

                    b.HasIndex("Host")
                        .IsUnique()
                        .HasFilter("[Host] IS NOT NULL");

                    b.ToTable("UrlHostStats");
                });
#pragma warning restore 612, 618
        }
    }
}