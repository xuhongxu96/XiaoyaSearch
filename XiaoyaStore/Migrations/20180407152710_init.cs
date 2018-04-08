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
                name: "InvertedIndices",
                columns: table => new
                {
                    InvertedIndexId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IndexType = table.Column<int>(nullable: false),
                    Position = table.Column<int>(nullable: false),
                    UrlFileId = table.Column<int>(nullable: false),
                    Word = table.Column<string>(type: "nvarchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvertedIndices", x => x.InvertedIndexId);
                });

            migrationBuilder.CreateTable(
                name: "UrlFileIndexStats",
                columns: table => new
                {
                    UrlFileIndexStatId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UrlFileId = table.Column<int>(nullable: false),
                    Weight = table.Column<double>(nullable: false),
                    Word = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    WordFrequency = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlFileIndexStats", x => x.UrlFileIndexStatId);
                });

            migrationBuilder.CreateTable(
                name: "UrlFiles",
                columns: table => new
                {
                    UrlFileId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Charset = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    FileHash = table.Column<string>(nullable: true),
                    FilePath = table.Column<string>(nullable: true),
                    IndexStatus = table.Column<int>(nullable: false),
                    MimeType = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    UpdateIntervalSeconds = table.Column<double>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    Url = table.Column<string>(nullable: true)
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
                name: "IX_InvertedIndices_UrlFileId",
                table: "InvertedIndices",
                column: "UrlFileId");

            migrationBuilder.CreateIndex(
                name: "IX_InvertedIndices_UrlFileId_Word_Position_IndexType",
                table: "InvertedIndices",
                columns: new[] { "UrlFileId", "Word", "Position", "IndexType" },
                unique: true,
                filter: "[Word] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFileIndexStats_UrlFileId",
                table: "UrlFileIndexStats",
                column: "UrlFileId");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFileIndexStats_Weight",
                table: "UrlFileIndexStats",
                column: "Weight");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFileIndexStats_Word",
                table: "UrlFileIndexStats",
                column: "Word");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFileIndexStats_WordFrequency",
                table: "UrlFileIndexStats",
                column: "WordFrequency");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFileIndexStats_Word_UrlFileId",
                table: "UrlFileIndexStats",
                columns: new[] { "Word", "UrlFileId" },
                unique: true,
                filter: "[Word] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFileIndexStats_Word_UrlFileId_Weight_WordFrequency",
                table: "UrlFileIndexStats",
                columns: new[] { "Word", "UrlFileId", "Weight", "WordFrequency" });

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
                name: "IX_UrlFiles_Url",
                table: "UrlFiles",
                column: "Url",
                unique: true,
                filter: "[Url] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFiles_UpdatedAt_IndexStatus",
                table: "UrlFiles",
                columns: new[] { "UpdatedAt", "IndexStatus" });

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

            migrationBuilder.Sql(@"CREATE VIEW dbo.IndexStats WITH SCHEMABINDING AS 
SELECT 
        Word,
        COUNT_BIG(*) AS DocumentFrequency,
		SUM(WordFrequency) AS WordFrequency
FROM dbo.UrlFileIndexStats
GROUP BY Word");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvertedIndices");

            migrationBuilder.DropTable(
                name: "UrlFileIndexStats");

            migrationBuilder.DropTable(
                name: "UrlFiles");

            migrationBuilder.DropTable(
                name: "UrlFrontierItems");

            migrationBuilder.DropTable(
                name: "UrlHostStats");
        }
    }
}
