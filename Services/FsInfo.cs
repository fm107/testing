﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using UTorrent.Api.Data;
using WebTorrent.Model;
using WebTorrent.Repository;

namespace WebTorrent.Services
{
    public class FsInfo
    {
        private readonly char[] _directorySeparators = {'\u005C', '\u002F'};
        private const string UploadFolder = "uploads";
        private readonly IHostingEnvironment _environment;
        private readonly IContentRecordRepository _repository;

        public FsInfo(IHostingEnvironment environment, IContentRecordRepository repository)
        {
            _environment = environment;
            _repository = repository;

            if (!Directory.Exists(Path.Combine(_environment.WebRootPath, UploadFolder)))
                Directory.CreateDirectory(Path.Combine(_environment.WebRootPath, UploadFolder));
        }

        public async Task<IList<Content>> GetFolderContent(string folder, bool needFiles, string hash)
        {
            return !string.IsNullOrEmpty(folder) && folder.Contains(UploadFolder)
                ? await _repository.FindByFolder(folder, needFiles, hash)
                : await _repository.FindByFolder(UploadFolder, needFiles, hash);
        }

        public async Task<Content> SaveFolderContent(UTorrent.Api.Data.Torrent torrent, ICollection<FileCollection> collection)
        {
            var content = await _repository.FindByHash(torrent.Hash, false, "FsItems");

            if (content != null) return content;

            var directoryInfo = new DirectoryInfo(torrent.Path);

            var fsContent = new List<FileSystemItem>();

            foreach (var col in collection)
            {
                fsContent.AddRange(col.Select(file => new FileSystemItem
                {
                    DownloadPath = Path.Combine(directoryInfo.FullName.Replace(_environment.WebRootPath, string.Empty), file.NameWithoutPath),
                    Name = file.NameWithoutPath,
                    FullName = directoryInfo.FullName,
                    LastChanged = DateTime.Now,
                    Size = file.Size,
                    Type = "file"
                }));
            }

            var folder = new FileSystemItem
            {
                Name = Path.ChangeExtension(torrent.Name, null),
                LastChanged = DateTime.Now,
                Size = fsContent.Sum(f => f.Size),
                Type = "folder",
                FullName = directoryInfo.FullName
            };

            fsContent.Add(folder);

            GetFolderPath(directoryInfo, out string currentFolder, out string parentFolder);

            content = new Content
            {
                FsItems = fsContent,
                CurrentFolder = currentFolder,
                ParentFolder = parentFolder,
                Hash = torrent.Hash,
                IsInProgress = true
            };

            _repository.Add(content);
            _repository.Save();

            return content;
        }

        private void GetFolderPath(FileSystemInfo path, out string currentFolder, out string parentFolder)
        {
            var separator = path.FullName.FirstOrDefault(c => _directorySeparators.Contains(c));

            var tmpString = path.FullName.Replace(_environment.WebRootPath, string.Empty)
                                         .TrimStart(_directorySeparators);
            var indexOfSeparator = tmpString.IndexOf(separator);
            var indexToRemove = tmpString.IndexOf(separator, indexOfSeparator + 1);

            string parrent;
            string current;
            if (indexToRemove > 0)
            {
                current = tmpString.Remove(indexToRemove);
                parrent = current.Substring(0, indexOfSeparator);
            }
            else
            {
                current = tmpString;
                parrent = current.Substring(0, indexOfSeparator);
            }

            currentFolder = current;
            parentFolder = parrent;
        }
    }
}