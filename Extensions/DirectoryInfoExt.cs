using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebTorrent.Extensions
{
    public static class DirectoryInfoExt
    {
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, SearchOption option,
            params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException(nameof(extensions));
            var files = Enumerable.Empty<FileInfo>();
            return extensions.Aggregate(files, (current, ext) => current.Concat(dir.GetFiles(ext, option)));
        }
    }
}