using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using MimeMapping;
using WebTorrent.Extensions;
using WebTorrent.Model;

namespace WebTorrent.Services
{
    public class FsInfo
    {
        private const string UploadFolder = "uploads";
        private readonly IHostingEnvironment _environment;

        public FsInfo(IHostingEnvironment environment)
        {
            _environment = environment;

            if (!Directory.Exists(Path.Combine(_environment.WebRootPath, UploadFolder)))
                Directory.CreateDirectory(Path.Combine(_environment.WebRootPath, UploadFolder));
        }

        public Content GetFolderContent(string folder)
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(_environment.WebRootPath, folder ?? UploadFolder));

            var fsItems = new List<FileSystemItem>(directoryInfo
                .GetFilesByExtensions(SearchOption.TopDirectoryOnly, MimeTypes.TypeMap
                    .Where(t => t.Value.Contains("video") | t.Value.Contains("audio"))
                    .Select(f => "*." + f.Key)
                    .ToArray())
                .Select(file => new FileSystemItem
                {
                    FullName = file.FullName.Replace(_environment.WebRootPath, string.Empty)
                        .TrimStart('\u005C', '\u002F'),
                    Name = file.Name,
                    Size = file.Length,
                    LastChanged = file.LastWriteTime,
                    Type = "file"
                }));

            fsItems.AddRange(directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly)
                .Select(directory => new FileSystemItem
                    {
                        FullName = directory.FullName.Replace(_environment.WebRootPath, string.Empty)
                            .TrimStart('\u005C', '\u002F'),
                        Name = directory.Name,
                        Size = new DirectoryInfo(directory.FullName).GetFiles("*", SearchOption.AllDirectories)
                            .Sum(f => f.Length),
                        LastChanged = directory.LastWriteTime,
                        Type = "folder"
                    }
                ));

            var content = new Content
            {
                CurrentFolder = folder ?? UploadFolder,
                Contents = fsItems,
                Parent = folder == null || folder == UploadFolder
                    ? null
                    : directoryInfo.Parent.FullName.Replace(_environment.WebRootPath, string.Empty)
                        .TrimStart('\u005C', '\u002F')
            };

            return content;
        }
    }
}