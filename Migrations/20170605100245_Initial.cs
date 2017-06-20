using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebTorrent.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Content",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CurrentFolder = table.Column<string>(nullable: true),
                    Hash = table.Column<string>(nullable: true),
                    IsInProgress = table.Column<bool>(nullable: false),
                    ParentFolder = table.Column<string>(nullable: true),
                    TorrentName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Content", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FsItem",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentId = table.Column<int>(nullable: true),
                    DownloadPath = table.Column<string>(nullable: true),
                    FullName = table.Column<string>(nullable: true),
                    IsStreaming = table.Column<bool>(nullable: false),
                    LastChanged = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Size = table.Column<long>(nullable: false),
                    Stream = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FsItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FsItem_Content_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Content",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FsItem_ContentId",
                table: "FsItem",
                column: "ContentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FsItem");

            migrationBuilder.DropTable(
                name: "Content");
        }
    }
}
