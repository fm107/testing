using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Torrent.Client;
using Torrent.Client.Events;
using WebTorrent.Extensions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebTorrent.Controllers
{
    [Route("api/[controller]")]
    public class TorrentController : Controller
    {
        private readonly HttpClient _client;
        private readonly IHostingEnvironment _environment;
        private readonly ILog _log;
        private string _fileName;
        private TorrentTransfer _torrent;

        public TorrentController(IHostingEnvironment environment)
        {
            _environment = environment;
            _client = new HttpClient();
            _log = LogManager.GetLogger(Assembly.GetEntryAssembly(), "TorrentController");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UploadFile(ICollection<IFormFile> file, string folder)
        {
            var uploads = Path.Combine(_environment.WebRootPath, folder);

            foreach (var uploadedFile in file)
            {
                if (uploadedFile.Length <= 0) continue;
                _fileName = Path.Combine(uploads, uploadedFile.FileName.Split('\\').LastOrDefault());

                using (var fileStream = new FileStream(_fileName, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
            }

            _log.Info("Starting torrent manager");
            _log.InfoFormat("file path is {0}", _fileName);

            try
            {
                _torrent = new TorrentTransfer(_fileName, uploads);
                _torrent.StateChanged += TorrentStateChanged;
                _torrent.ReportStats += TorrentReportStats;
                _torrent.Start();
            }
            catch (Exception exception)
            {
                _log.Error(exception);
            }

            return Ok("{}");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UploadFileUrl([FromQuery] string url, [FromQuery] string folder)
        {
            var response = await _client.GetAsync(url, HttpCompletionOption.ResponseContentRead);

            var uploads = Path.Combine(_environment.WebRootPath, folder);

            _fileName = Path.Combine(uploads,
                response.Content.Headers.ContentDisposition != null
                    ? response.Content.Headers.ContentDisposition.FileName.Trim('\u0022')
                    : response.RequestMessage.RequestUri.Segments.LastOrDefault());

            using (var fileStream = new FileStream(_fileName, FileMode.Create))
            {
                await response.Content.CopyToAsync(fileStream);
            }

            _log.Info("Starting torrent manager");
            _log.InfoFormat("file path is {0}", _fileName);

            try
            {
                _torrent = new TorrentTransfer(_fileName, uploads);
                _torrent.StateChanged += TorrentStateChanged;
                _torrent.ReportStats += TorrentReportStats;
                _torrent.Start();
            }
            catch (Exception exception)
            {
                _log.Error(exception);
            }

            return Ok(Path.GetFileName(_fileName));
        }

        private void TorrentReportStats(object sender, StatsEventArgs e)
        {
            Console.WriteLine("Total peers: " + e.TotalPeers);
        }

        private void TorrentStateChanged(object sender, EventArgs<TorrentState> e)
        {
            switch (e.Value)
            {
                case TorrentState.Seeding:
                    _torrent.Stop();
                    _torrent.StateChanged -= TorrentStateChanged;
                    _log.Info("Starting ffmpeg");
                    try
                    {
                        foreach (var file in _torrent.Data.Files)
                            if (file.IsMedia() && Path.GetExtension(file.Name) != ".mp4")
                            {
                                var fileToConvert = Directory.EnumerateFiles(_environment.ContentRootPath, file.Name,
                                        SearchOption.AllDirectories)
                                    .FirstOrDefault();

                                var processInfo = new ProcessStartInfo("ffmpeg")
                                {
                                    Arguments = string.Format(@"-i {0} -f mp4 -vcodec libx264 -preset ultrafast 
                                                                -movflags faststart -profile:v main -acodec aac {1} -hide_banner", 
                                                                fileToConvert, string.Format("{0}.mp4", Path.ChangeExtension(fileToConvert, null)))
                                };

                                Process.Start(processInfo);

                                System.IO.File.Delete(fileToConvert);

                                _log.InfoFormat("file {0} has been converted",
                                    Path.GetFileNameWithoutExtension(file.Name));
                            }
                    }
                    catch (Exception exception)
                    {
                        _log.Error(exception);
                    }

                    break;
            }
        }
    }
}