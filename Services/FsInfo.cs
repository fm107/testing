using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using MimeMapping;
using UTorrent.Api.Data;
using WebTorrent.Extensions;
using WebTorrent.Model;
using WebTorrent.Repository;

namespace WebTorrent.Services
{
    public class FsInfo
    {
        private const string UploadFolder = "uploads";
        private readonly IHostingEnvironment _environment;
        private readonly IContentRecordRepository _repository;
        private TorrentClient _torrentClient;

        public FsInfo(IHostingEnvironment environment, IContentRecordRepository repository)
        {
            _environment = environment;
            _repository = repository;

            if (!Directory.Exists(Path.Combine(_environment.WebRootPath, UploadFolder)))
                Directory.CreateDirectory(Path.Combine(_environment.WebRootPath, UploadFolder));
        }

        public IList<Content> GetFolderContent(string folder)
        {
            if (!string.IsNullOrEmpty(folder) && folder.Contains(@"C:\Users\Alexander\Downloads\wwwroot\uploads"))
            {
                var Info = Path.Combine(_environment.WebRootPath, folder);
                return _repository.Find(Info);
            }

            return _repository.Find(@"C:\Users\Alexander\Downloads\wwwroot\uploads");

            //var directoryInfo = new DirectoryInfo(Path.Combine(_environment.WebRootPath, folder ?? UploadFolder));

            //var fsContent = new List<FileSystemItem>(directoryInfo
            //    .GetFilesByExtensions(SearchOption.TopDirectoryOnly, MimeTypes.TypeMap
            //        .Where(t => t.Value.Contains("video") | t.Value.Contains("audio"))
            //        .Select(f => "*." + f.Key)
            //        .ToArray())
            //    .Select(file => new FileSystemItem
            //    {
            //        FullName = file.FullName.Replace(_environment.WebRootPath, string.Empty)
            //            .TrimStart('\u005C', '\u002F'),
            //        Name = file.Name,
            //        Size = file.Length,
            //        LastChanged = file.LastWriteTime,
            //        Type = "file"
            //    }));

            //fsContent.AddRange(directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly)
            //    .Select(directory => new FileSystemItem
            //    {
            //        FullName = directory.FullName.Replace(_environment.WebRootPath, string.Empty)
            //                .TrimStart('\u005C', '\u002F'),
            //        Name = directory.Name,
            //        Size = new DirectoryInfo(directory.FullName).GetFiles("*", SearchOption.AllDirectories)
            //                .Sum(f => f.Length),
            //        LastChanged = directory.LastWriteTime,
            //        Type = "folder"
            //    }
            //    ));

            //var content = new Content
            //{
            //    CurrentFolder = folder ?? UploadFolder,
            //    FsItems = fsContent,
            //    ParentFolder = folder == null || folder == UploadFolder
            //        ? null
            //        : directoryInfo.Parent.FullName.Replace(_environment.WebRootPath, string.Empty)
            //            .TrimStart('\u005C', '\u002F')
            //};

            //return content;
        }

        public Content SaveFolderContent(UTorrent.Api.Data.Torrent torrent, ICollection<FileCollection> collection)
        {

            var directoryInfo = new DirectoryInfo(torrent.Path);

            var fsContent = new List<FileSystemItem>();

            foreach (var col in collection)
            {
                fsContent.AddRange(col.Select(file => new FileSystemItem
                {
                    Name = file.NameWithoutPath,
                    FullName = Path.Combine(directoryInfo.FullName, Path.ChangeExtension(torrent.Name, null)),
                    LastChanged = DateTime.Now,
                    Size = file.Size,
                    Type = "file"
                }));
            }

            var folder = new FileSystemItem();
            folder.Name = Path.ChangeExtension(torrent.Name, null);
            folder.LastChanged = DateTime.Now;
            folder.Size = fsContent.Sum(f => f.Size);
            folder.Type = "folder";
            folder.FullName = Path.Combine(directoryInfo.FullName, folder.Name);

            fsContent.Add(folder);

            var content = new Content
            {
                FsItems = fsContent,
                CurrentFolder = folder.FullName ?? UploadFolder,
                ParentFolder = torrent.Path == null || torrent.Path == UploadFolder
                    ? null
                    : directoryInfo.Parent.FullName.Replace(_environment.WebRootPath, string.Empty)
                        .TrimStart('\u005C', '\u002F'),
                Hash = torrent.Hash,
                IsInProgress = true
            };

            _repository.Add(content);
            _repository.Save();

            return content;
        }
    }
}