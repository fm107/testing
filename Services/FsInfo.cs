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

#if DEBUG
        public async Task<IList<Content>> GetFolderContent(string folder)
        {
            if (!string.IsNullOrEmpty(folder) && folder.Contains(@"C:\Users\Alexander\Downloads\wwwroot\uploads"))
            {
                var Info = Path.Combine(_environment.WebRootPath, folder);
                return await _repository.Find(Info);
            }

            return await _repository.Find(@"C:\Users\Alexander\Downloads\wwwroot\uploads");
        }

#else
        public async Task<IList<Content>> GetFolderContent(string folder)
        {
            if (!string.IsNullOrEmpty(folder) && folder.Contains("wwwroot/uploads"))
            {
                var Info = Path.Combine(_environment.WebRootPath, folder);
                return await _repository.Find(Info);
            }

            return await _repository.Find(@"/app/heroku_output/wwwroot/uploads");
        }
#endif

        public Content SaveFolderContent(UTorrent.Api.Data.Torrent torrent, ICollection<FileCollection> collection)
        {
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