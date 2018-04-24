using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace XiaoyaStore.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndexStats",
                columns: table => new
                {
                    IndexStatId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DocumentFrequency = table.Column<long>(nullable: false),
                    Word = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    WordFrequency = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexStats", x => x.IndexStatId);
                });

            migrationBuilder.CreateTable(
                name: "InvertedIndices",
                columns: table => new
                {
                    InvertedIndexId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OccurencesInLinks = table.Column<int>(nullable: false),
                    OccurencesInTitle = table.Column<int>(nullable: false),
                    Positions = table.Column<string>(nullable: true),
                    UrlFileId = table.Column<int>(nullable: false),
                    Weight = table.Column<double>(nullable: false),
                    Word = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    WordFrequency = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvertedIndices", x => x.InvertedIndexId);
                });

            migrationBuilder.CreateTable(
                name: "Links",
                columns: table => new
                {
                    LinkId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Text = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    UrlFileId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Links", x => x.LinkId);
                });

            migrationBuilder.CreateTable(
                name: "UrlFiles",
                columns: table => new
                {
                    UrlFileId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Charset = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    Content = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(300)", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(300)", nullable: true),
                    IndexStatus = table.Column<int>(nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    PageRank = table.Column<double>(nullable: false),
                    PublishDate = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    UpdateIntervalSeconds = table.Column<double>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    Url = table.Column<string>(type: "nvarchar(300)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlFiles", x => x.UrlFileId);
                });

            migrationBuilder.CreateTable(
                name: "UrlFrontierItems",
                columns: table => new
                {
                    UrlFrontierItemId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    FailedTimes = table.Column<int>(nullable: false),
                    Host = table.Column<string>(nullable: true),
                    IsPopped = table.Column<bool>(nullable: false),
                    PlannedTime = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    UrlDepth = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlFrontierItems", x => x.UrlFrontierItemId);
                });

            migrationBuilder.CreateTable(
                name: "UrlHostStats",
                columns: table => new
                {
                    UrlHostStatId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Count = table.Column<int>(nullable: false),
                    Host = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlHostStats", x => x.UrlHostStatId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndexStats_Word",
                table: "IndexStats",
                column: "Word",
                unique: true,
                filter: "[Word] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IndexStats_WordFrequency",
                table: "IndexStats",
                column: "WordFrequency");

            migrationBuilder.CreateIndex(
                name: "IX_InvertedIndices_UrlFileId",
                table: "InvertedIndices",
                column: "UrlFileId");

            migrationBuilder.CreateIndex(
                name: "IX_InvertedIndices_Weight",
                table: "InvertedIndices",
                column: "Weight");

            migrationBuilder.CreateIndex(
                name: "IX_InvertedIndices_Word",
                table: "InvertedIndices",
                column: "Word");

            migrationBuilder.CreateIndex(
                name: "IX_InvertedIndices_Word_UrlFileId",
                table: "InvertedIndices",
                columns: new[] { "Word", "UrlFileId" },
                unique: true,
                filter: "[Word] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InvertedIndices_Word_Weight",
                table: "InvertedIndices",
                columns: new[] { "Word", "Weight" });

            migrationBuilder.CreateIndex(
                name: "IX_Links_Url",
                table: "Links",
                column: "Url");

            migrationBuilder.CreateIndex(
                name: "IX_Links_UrlFileId",
                table: "Links",
                column: "UrlFileId");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFiles_FileHash",
                table: "UrlFiles",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFiles_FilePath",
                table: "UrlFiles",
                column: "FilePath",
                unique: true,
                filter: "[FilePath] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFiles_IndexStatus",
                table: "UrlFiles",
                column: "IndexStatus");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFiles_PageRank",
                table: "UrlFiles",
                column: "PageRank");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFiles_PublishDate",
                table: "UrlFiles",
                column: "PublishDate");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFiles_Url",
                table: "UrlFiles",
                column: "Url",
                unique: true,
                filter: "[Url] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFiles_IndexStatus_UpdatedAt",
                table: "UrlFiles",
                columns: new[] { "IndexStatus", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UrlFrontierItems_IsPopped",
                table: "UrlFrontierItems",
                column: "IsPopped");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFrontierItems_Url",
                table: "UrlFrontierItems",
                column: "Url",
                unique: true,
                filter: "[Url] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFrontierItems_PlannedTime_IsPopped",
                table: "UrlFrontierItems",
                columns: new[] { "PlannedTime", "IsPopped" });

            migrationBuilder.CreateIndex(
                name: "IX_UrlHostStats_Host",
                table: "UrlHostStats",
                column: "Host",
                unique: true,
                filter: "[Host] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndexStats");

            migrationBuilder.DropTable(
                name: "InvertedIndices");

            migrationBuilder.DropTable(
                name: "Links");

            migrationBuilder.DropTable(
                name: "UrlFiles");

            migrationBuilder.DropTable(
                name: "UrlFrontierItems");

            migrationBuilder.DropTable(
                name: "UrlHostStats");
        }
    }
}
