using System;
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

            if (content!=null)
            {
                return content;
            }

            var directoryInfo = new DirectoryInfo(torrent.Path);

            var fsContent = new List<FileSystemItem>();

            foreach (var col in collection)
                fsContent.AddRange(col.Select(file => new FileSystemItem
                {
                    DownloadPath = directoryInfo.FullName.Replace(_environment.WebRootPath, string.Empty),
                    Name = file.NameWithoutPath,
                    FullName = Path.Combine(directoryInfo.FullName, Path.ChangeExtension(torrent.Name, null)),
                    LastChanged = DateTime.Now,
                    Size = file.Size,
                    Type = "file"
                }));

            var folder = new FileSystemItem
            {
                Name = Path.ChangeExtension(torrent.Name, null),
                LastChanged = DateTime.Now,
                Size = fsContent.Sum(f => f.Size),
                Type = "folder",
                FullName = directoryInfo.FullName
            };

            fsContent.Add(folder);

            content = new Content
            {
                FsItems = fsContent,
                CurrentFolder = directoryInfo.FullName.Replace(_environment.WebRootPath, string.Empty).TrimStart('\u005C', '\u002F'),
                ParentFolder = directoryInfo.Parent.FullName.Replace(_environment.WebRootPath, string.Empty).TrimStart('\u005C', '\u002F'),
                Hash = torrent.Hash,
                IsInProgress = true
            };

            _repository.Add(content);
            _repository.Save();

            return content;
        }
    }
}