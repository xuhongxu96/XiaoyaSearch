using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace XiaoyaStore.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndexStats",
                columns: table => new
                {
                    IndexStatId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocumentFrequency = table.Column<long>(nullable: false),
                    Word = table.Column<string>(nullable: true),
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
                        .Annotation("Sqlite:Autoincrement", true),
                    IndexType = table.Column<int>(nullable: false),
                    Position = table.Column<int>(nullable: false),
                    UrlFileId = table.Column<int>(nullable: false),
                    Word = table.Column<string>(nullable: true)
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
                        .Annotation("Sqlite:Autoincrement", true),
                    UrlFileId = table.Column<int>(nullable: false),
                    Word = table.Column<string>(nullable: true),
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
                        .Annotation("Sqlite:Autoincrement", true),
                    Charset = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    FileHash = table.Column<string>(nullable: true),
                    FilePath = table.Column<string>(nullable: true),
                    IsIndexed = table.Column<bool>(nullable: false),
                    MimeType = table.Column<string>(nullable: true),
                    UpdateInterval = table.Column<TimeSpan>(nullable: false),
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
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    FailedTimes = table.Column<int>(nullable: false),
                    IsPopped = table.Column<bool>(nullable: false),
                    PlannedTime = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlFrontierItems", x => x.UrlFrontierItemId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndexStats_Word",
                table: "IndexStats",
                column: "Word",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvertedIndices_UrlFileId_Word_Position_IndexType",
                table: "InvertedIndices",
                columns: new[] { "UrlFileId", "Word", "Position", "IndexType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrlFileIndexStats_WordFrequency",
                table: "UrlFileIndexStats",
                column: "WordFrequency");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFileIndexStats_Word_UrlFileId",
                table: "UrlFileIndexStats",
                columns: new[] { "Word", "UrlFileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrlFiles_FilePath",
                table: "UrlFiles",
                column: "FilePath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrlFiles_UpdatedAt",
                table: "UrlFiles",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UrlFiles_Url",
                table: "UrlFiles",
                column: "Url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrlFrontierItems_Url",
                table: "UrlFrontierItems",
                column: "Url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrlFrontierItems_PlannedTime_IsPopped",
                table: "UrlFrontierItems",
                columns: new[] { "PlannedTime", "IsPopped" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndexStats");

            migrationBuilder.DropTable(
                name: "InvertedIndices");

            migrationBuilder.DropTable(
                name: "UrlFileIndexStats");

            migrationBuilder.DropTable(
                name: "UrlFiles");

            migrationBuilder.DropTable(
                name: "UrlFrontierItems");
        }
    }
}
