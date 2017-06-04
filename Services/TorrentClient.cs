﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
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
        private readonly IHostingEnvironment _environment;
        private readonly FFmpeg _ffmpeg;
        private readonly FsInfo _fsInfo;
        private readonly TaskSynchronizationScope _lock = new TaskSynchronizationScope();
        private readonly ILogger<TorrentClient> _log;
        private readonly IContentRecordRepository _repository;
        private Timer _timer;

        public TorrentClient(FsInfo fsInfo, IContentRecordRepository repository, ILogger<TorrentClient> log,
            FFmpeg ffmpeg, IHostingEnvironment environment)
        {
            _fsInfo = fsInfo;
            _repository = repository;
            _log = log;
            _ffmpeg = ffmpeg;
            _environment = environment;
            _client = new UTorrentClient("admin", "");
            _timer = new Timer(CheckStatus, null, 0, (int) TimeSpan.FromSeconds(10).TotalMilliseconds);
        }

        private async void CheckStatus(object state)
        {
            foreach (var tor in _client.GetList().Result.Torrents)
            {
                if (tor.Progress != 1000)
                {
                    continue;
                }

                Content content = null;
                await _lock.RunAsync(async () => { content = await _repository.FindByHash(tor.Hash, true); });

                if (content?.IsInProgress != true)
                {
                    continue;
                }

                _log.LogInformation("Creating playing list for {0}", tor.Name);
                ChangeStatus(content);
                CreatePlayList(content);
            }
        }

        private void CreatePlayList(Content content)
        {
            foreach (var file in content.FsItems.Where(f => f.Type.Equals("file")))
            {
                if (MimeTypes.GetMimeMapping(file.Name).Contains("video") |
                    MimeTypes.GetMimeMapping(file.Name).Contains("audio"))
                {
                    var fileToConvert = Path.Combine(file.FullName, file.Name);

                    _log.LogInformation("Start convert process for {0}", fileToConvert);
                    _log.LogInformation("file path {0}", file.FullName);

                    file.Stream = Path.Combine(file.FullName.Replace(_environment.WebRootPath, string.Empty),
                        file.Name + ".m3u8");
                    file.IsStreaming = true;
                    _repository.Update(content);
                    _repository.Save();

                    _ffmpeg.CreatePlayList(fileToConvert, file.FullName, file.Name);
                }
            }
        }

        private void ChangeStatus(Content content)
        {
            content.IsInProgress = false;

            _repository.Update(content);
            _repository.Save();
        }

        private void CreatePlayList(UTorrent.Api.Data.Torrent tor)
        {
            foreach (var files in _client.GetFiles(tor.Hash).Result.Files.Values)
            {
                foreach (var file in files)
                {
                    if (MimeTypes.GetMimeMapping(file.Name).Contains("video") |
                    MimeTypes.GetMimeMapping(file.Name).Contains("audio"))
                    {
                        if (!file.NameWithoutPath.EndsWith(".mp4") &&
                        tor.Path.Contains(Path.ChangeExtension(file.NameWithoutPath, null)))
                    {
                        var fileToConvert = Path.Combine(tor.Path, file.Name);

                        _log.LogInformation("Start convert process for {0}", fileToConvert);
                        _log.LogInformation("file path {0}", tor.Path);

                        _ffmpeg.CreatePlayList(fileToConvert, tor.Path, "hkhgkipj");

                        //     Task.Factory.StartNew(() =>
                        //{
                        //    var fileToConvert = Path.Combine(tor.Path, file.Name);

                        //    _log.LogInformation("Start convert process for {0}", fileToConvert);
                        //    _log.LogInformation("file path {0}", tor.Path);
                        //    var processInfo = new ProcessStartInfo(@"/app/vendor/ffmpeg/ffmpeg")
                        //    {
                        //        Arguments = string.Format(
                        //            @"-i {0} -codec:v libx264 -codec:a aac -map 0 -f segment -segment_time 10 -segment_format mpegts -segment_list_flags live -segment_list {1}/out.m3u8 -segment_list_type m3u8 {1}/%d.ts",
                        //            fileToConvert, tor.Path)
                        //    };

                        //    var process = Process.Start(processInfo);
                        //    process.WaitForExit();
                        //     //await _client.DeleteTorrentAsync(tor.Hash);
                        //     //await _repository.Delete((await _repository.FindByHash(tor.Hash)).Id);
                        //     //File.Delete(fileToConvert);
                        // });
                    }
                    }
                }
            }
        }

        public async Task<Content> AddTorrent(Stream file, string path)
        {
            var response = _client.PostTorrent(file, Path.Combine(path, await GetTorrentName(file)));
            var torrent = response.AddedTorrent;
            return await _fsInfo.SaveFolderContent(torrent, await GetFiles(torrent.Hash));
        }

        public async Task<Content> AddUrlTorrent(string url, string path)
        {
            var response = _client.AddUrlTorrent(url, Path.Combine(path, GetTorrentName(url)));
            var torrent = response.AddedTorrent;
            return await _fsInfo.SaveFolderContent(torrent, await GetFiles(torrent.Hash));
        }

        private static string GetTorrentName(string file)
        {
            var torrentName = file.ToCharArray(20, 40);
            return string.Concat(torrentName);
        }

        private static async Task<string> GetTorrentName(Stream file)
        {
            var buffer = new byte[file.Length];
            var regex = new Regex(":name[0-9]{2}:(.*?)[0-9]{2}:piece");

            if (file.CanRead)
            {
                await file.ReadAsync(buffer, 0, buffer.Length);
                if (file.CanSeek)
                {
                    file.Seek(0, SeekOrigin.Begin);
                }
            }
            var stringOfStream = Encoding.UTF8.GetString(buffer);
            return Path.ChangeExtension(regex.Match(stringOfStream).Groups[1].Value, null);
        }

        public async Task<bool> IsTorrentType(Stream file)
        {
            var buffer = new byte[11];
            var torrentType = new byte[] {0x64, 0x38, 0x3a, 0x61, 0x6e, 0x6e, 0x6f, 0x75, 0x6e, 0x63, 0x65};

            if (file.CanRead)
            {
                await file.ReadAsync(buffer, 0, buffer.Length);
                if (file.CanSeek)
                {
                    file.Seek(0, SeekOrigin.Begin);
                }
            }

            return buffer.SequenceEqual(torrentType);
        }

        public List<object> GetTorrent()
        {
            //var set = new MediaEncodingSetup(@"d:\temp\Vojna.2016.D.BDRip_ExKinoRay_by_Twi7ter.avi", @"D:\Temp\New folder\output.mp4", EVideoHeight.P1080);

            //Fbx.VideoConverter.FFMpegEncoderTask tsk = new FFMpegEncoderTask(set);
            //tsk.ConvertProgressEvent += Tsk_ConvertProgressEvent;
            //tsk.Convert();

            //var settings = new ConvertSettings();

            //var ffMpeg = new FFMpegConverter();
            //ffMpeg.FFMpegExeName = @"ffmpeg.exe";
            //ffMpeg.FFMpegToolPath = @"D:\Temp\GitHub\testing\bin\Debug\netcoreapp1.1\";
            //var mp4Stream = new FileStream(@"D:\Temp\New folder\output.mp4", FileMode.Create);

            ////ffMpeg.ConvertMedia(@"d:\temp\Vojna.2016.D.BDRip_ExKinoRay_by_Twi7ter.avi", @"D:\Temp\New folder\output.mp4", Format.mp4);
            //var task = ffMpeg.ConvertLiveMedia(@"d:\temp\Vojna.2016.D.BDRip_ExKinoRay_by_Twi7ter.avi", "avi", mp4Stream, "mp4",
            //    settings);
            //task.Start();
            //task.Wait();

            var fileToConvert = @"D:\Temp\Vojna.2016.D.BDRip_ExKinoRay_by_Twi7ter.avi";

            var processInfo = new ProcessStartInfo(@"D:\Temp\GitHub\testing\bin\Debug\netcoreapp1.1\ffmpeg")
            {
                Arguments = string.Format(
                    @"-i C:\Users\Alexander\Downloads\wwwroot\uploads\Time.Toys.2016.P.WEB-DLRip.14OOMB.avi 
-codec:v libx264 -codec:a aac -map 0 
-f segment -segment_time 10 -segment_format mpegts -segment_list_flags live -segment_list output\out.m3u8 -segment_list_type m3u8 output\%d.ts",
                    fileToConvert, @"D:\Temp\New folder\output.mp4")
            };

            var process = Process.Start(processInfo);
            process.WaitForExit();
            //File.Delete(fileToConvert);


            var items = new List<object>();
            foreach (var torrent in _client.GetList().Result.Torrents)
            {
                items.Add(new {torrent.Name, Progress = torrent.Progress / 10.0, torrent.Remaining});
            }

            return items;
        }

        private async Task<ICollection<FileCollection>> GetFiles(string hash)
        {
            var response = await _client.GetFilesAsync(hash);
            return response.Result.Files.Values;
        }

        public async Task<string> GetTorrentInfo(string hash)
        {
            var torrent = await _client.GetTorrentAsync(hash);
            return torrent.Result.Torrents
                .Aggregate<UTorrent.Api.Data.Torrent, string>(null, (current, tor) =>
                    current + $"{tor.Name} " + Environment.NewLine +
                    $"Progress: {tor.Progress / 10.0} " + Environment.NewLine +
                    $"Path: {tor.Path} " + Environment.NewLine +
                    $"Remaining: {tor.Remaining}" + Environment.NewLine);
        }

        private void ConvertVideo(object state)
        {
            foreach (var tor in _client.GetList().Result.Torrents)
            {
                if (tor.Progress != 1000)
                {
                    continue;
                }

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
    }
}