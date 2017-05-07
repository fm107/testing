using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MimeMapping;
using UTorrent.Api;
using UTorrent.Api.Data;
using WebTorrent.Model;
using WebTorrent.Repository;

namespace WebTorrent.Services
{
    public class TorrentClient
    {
        private readonly UTorrentClient _client;
        private Timer _timer;
        private readonly FsInfo _fsInfo;
        private readonly IContentRecordRepository _repository;
        public TorrentClient(FsInfo fsInfo, IContentRecordRepository repository)
        {
            _fsInfo = fsInfo;
            _repository = repository;
            _client = new UTorrentClient("admin", "");
            //_timer = new Timer(ConvertVideo, null, 0, (int)TimeSpan.FromSeconds(10).TotalMilliseconds);
        }

        public async Task<Content> AddTorrent(Stream file, string path)
        {
            var response = _client.PostTorrent(file, path);
            var torrent = response.AddedTorrent;
            var content =  _fsInfo.SaveFolderContent(torrent, await GetFiles(torrent.Hash));
            return content;
        }

        public UTorrent.Api.Data.Torrent AddUrlTorrent(string url, string path)
        {
            var response = _client.AddUrlTorrent(url, path);
            return response.AddedTorrent;
        }

        public bool IsTorrentType(Stream file)
        {
            var buffer = new byte[11];
            var torrentType = new byte[] { 0x64, 0x38, 0x3a, 0x61, 0x6e, 0x6e, 0x6f, 0x75, 0x6e, 0x63, 0x65 };

            if (file.CanRead)
            {
                file.ReadAsync(buffer, 0, buffer.Length);
                if (file.CanSeek)
                    file.Seek(0, SeekOrigin.Begin);
            }

            return buffer.SequenceEqual(torrentType);
        }

        public List<object> GetTorrent()
        {
            var items = new List<object>();
            foreach (var torrent in _client.GetList().Result.Torrents)
            {
                items.Add(new {torrent.Name, Progress = torrent.Progress / 10.0, torrent.Remaining});
            }
            return items;
        }

        public async Task<ICollection<FileCollection>> GetFiles(string hash)
        {
            var response = await _client.GetFilesAsync(hash);
            return response.Result.Files.Values;
        }

        public string GetTorrentInfo()
        {
            return _client.GetList().Result.Torrents
                .Aggregate<UTorrent.Api.Data.Torrent, string>(null, (current, tor) => 
                current + $"{tor.Name} " + Environment.NewLine +
                $"Progress: {tor.Progress / 10.0} " + Environment.NewLine +
                $"Path: {tor.Path} " + Environment.NewLine +
                $"Remaining: {tor.Remaining}" + Environment.NewLine + 
                new string('-', 20) + Environment.NewLine);
        }

        public void ConvertVideo(object state)
        {
            foreach (var tor in _client.GetList().Result.Torrents)
            {
                if (tor.Progress != 1000) continue;

                foreach (var files in _client.GetFiles(tor.Hash).Result.Files.Values)
                {
                    foreach (var file in files)
                    {
                        if (MimeTypes.GetMimeMapping(file.Name).Contains("video") |
                            MimeTypes.GetMimeMapping(file.Name).Contains("audio"))
                        {
                            if (!file.NameWithoutPath.EndsWith(".mp4"))
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    var fileToConvert = Path.Combine(tor.Path, file.Name);

                                    var processInfo = new ProcessStartInfo("/app/vendor/ffmpeg/ffmpeg")
                                    {
                                        Arguments = string.Format(@"-i {0} -f mp4 -vcodec libx264 -preset ultrafast 
                                                                                     -movflags faststart -profile:v main -acodec aac {1} -hide_banner",
                                            fileToConvert,
                                            string.Format("{0}.mp4", Path.ChangeExtension(fileToConvert, null)))
                                    };

                                    var process = Process.Start(processInfo);
                                    process.WaitForExit();
                                    //File.Delete(fileToConvert);
                                });
                            }
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return GetHashCode().ToString();
        }
    }
}