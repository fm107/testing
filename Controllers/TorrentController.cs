using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Torrent.Client;
using Torrent.Client.Events;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebTorrent.Controllers
{
    [Route("api/[controller]")]
    public class TorrentController : Controller
    {
        private readonly HttpClient _client;
        private readonly IHostingEnvironment _environment;
        private string _fileName;
        private TorrentTransfer _torrent;

        public TorrentController(IHostingEnvironment environment)
        {
            _environment = environment;
            _client = new HttpClient();
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

            _torrent = new TorrentTransfer(_fileName, uploads);
            _torrent.StateChanged += TorrentStateChanged;
            _torrent.ReportStats += TorrentReportStats;
            _torrent.Start();

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

            _torrent = new TorrentTransfer(_fileName, uploads);
            _torrent.StateChanged += TorrentStateChanged;
            _torrent.ReportStats += TorrentReportStats;
            _torrent.Start();

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
                    var processInfo = new ProcessStartInfo("bash")
                    {
                        Arguments = string.Format("ffmpeg -i {0} -vcodec copy -acodec copy {1}", _fileName,
                            string.Format("{0}.mp4", Path.GetFileNameWithoutExtension(_fileName)))
                    };
                    Process.Start(processInfo);
                    break;
            }
        }
    }
}