using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using WebTorrent.Data;

namespace WebTorrent.Migrations
{
    [DbContext(typeof(ContentDbContext))]
    [Migration("20170605100245_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("WebTorrent.Model.Content", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CurrentFolder");

                    b.Property<string>("Hash");

                    b.Property<bool>("IsInProgress");

                    b.Property<string>("ParentFolder");

                    b.Property<string>("TorrentName");

                    b.HasKey("Id");

                    b.ToTable("Content");
                });

            modelBuilder.Entity("WebTorrent.Model.FileSystemItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ContentId");

                    b.Property<string>("DownloadPath");

                    b.Property<string>("FullName");

                    b.Property<bool>("IsStreaming");

                    b.Property<DateTime>("LastChanged");

                    b.Property<string>("Name");

                    b.Property<long>("Size");

                    b.Property<string>("Stream");

                    b.Property<string>("Type");

                    b.HasKey("Id");

                    b.HasIndex("ContentId");

                    b.ToTable("FsItem");
                });

            modelBuilder.Entity("WebTorrent.Model.FileSystemItem", b =>
                {
                    b.HasOne("WebTorrent.Model.Content")
                        .WithMany("FsItems")
                        .HasForeignKey("ContentId");
                });
        }
    }
}
