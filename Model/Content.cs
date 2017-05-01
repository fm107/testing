using System;
using System.Collections.Generic;

namespace WebTorrent.Model
{
    public class Content
    {
        public string CurrentFolder { get; set; }
        public string Parent { get; set; }
        public List<FileSystemItem> Contents { get; set; }
    }

    public class FileSystemItem
    {
        public string FullName { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime LastChanged { get; set; }
        public string Type { get; set; }
    }
}