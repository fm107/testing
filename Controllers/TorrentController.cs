using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebTorrent.Model;
using WebTorrent.Services;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebTorrent.Controllers
{
    [Route("api/[controller]")]
    public class TorrentController : Controller
    {
        private const string DownLoadFolder = @"wwwroot/uploads";
        private readonly HttpClient _client;
        private readonly IHostingEnvironment _environment;
        private readonly ILogger<TorrentController> _log;
        private readonly TorrentClient _torrentClient;

        private WebSocket _webSocket;

        public TorrentController(ILogger<TorrentController> log, IHostingEnvironment environment,
            TorrentClient torrentClient)
        {
            _environment = environment;
            _torrentClient = torrentClient;
            _client = new HttpClient();
            _log = log;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UploadFromUrl([FromQuery] string url, [FromQuery] string folder)
        {
            if (url.StartsWith("magnet:?xt=urn:btih:"))
            {
                var torrent = _torrentClient.AddUrlTorrent(url, DownLoadFolder);

                return Json(torrent.Name);
            }

            var response = await _client.GetAsync(url, HttpCompletionOption.ResponseContentRead);
            var content = await response.Content.ReadAsStreamAsync();

            if (!await _torrentClient.IsTorrentType(content))
            {
                return BadRequest("Not application/x-bittorrent Mime type");
            }

            return Json(_torrentClient.AddTorrent(content, DownLoadFolder));

            //var response = await _client.GetAsync(url, HttpCompletionOption.ResponseContentRead);

            //var uploads = Path.Combine(_environment.WebRootPath, folder);

            //_fileName = Path.Combine(uploads, RemoveInvalidFilePathCharacters(
            //    response.Content.Headers.ContentDisposition != null
            //        ? response.Content.Headers.ContentDisposition.FileName.Trim('\u0022')
            //        : response.RequestMessage.RequestUri.Segments.LastOrDefault(), "_"));

            //using (var fileStream = new FileStream(_fileName, FileMode.Create))
            //{
            //    await response.Content.CopyToAsync(fileStream);
            //}

            //if (MimeTypes.GetMimeMapping(_fileName) != "application/x-bittorrent")
            //    return BadRequest("Not application/x-bittorrent Mime type");

            //_log.Info("Starting torrent manager");
            //_log.InfoFormat("file path is {0}", _fileName);

            //try
            //{
            //    //_torrent = new TorrentTransfer(_fileName, uploads);
            //    //_torrent.StateChanged += TorrentStateChanged;
            //    //_torrent.ReportStats += TorrentReportStats;
            //    //_torrent.Start();


            //    //var response2 = client.PostTorrent(new FileStream(_fileName, FileMode.Open),
            //    //    Path.Combine("wwwroot/uploads", Path.GetFileNameWithoutExtension(_fileName)));
            //    //var torrent = response2.AddedTorrent;
            //}
            //catch (Exception exception)
            //{
            //    _log.Error(exception);
            //    throw;
            //}

            //return Ok(Path.GetFileName(_fileName));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UploadFile(ICollection<IFormFile> file, string folder)
        {
            foreach (var uploadedFile in file)
            {
                if (uploadedFile.Length <= 0)
                {
                    continue;
                }

                var content = uploadedFile.OpenReadStream();

                if (!await _torrentClient.IsTorrentType(content))
                {
                    return BadRequest("Not application/x-bittorrent Mime type");
                }

                return Json(string.IsNullOrEmpty(folder)
                    ? new List<Content> {await _torrentClient.AddTorrent(content, folder)}
                    : new List<Content> {await _torrentClient.AddTorrent(content, DownLoadFolder)});
            }

            return Ok("{}");
        }

        [HttpGet("[action]")]
        public string GetTorrentInfo()
        {
            return _torrentClient.GetTorrentInfo();
        }

        [HttpGet("[action]")]
        public IActionResult GetTorrent()
        {
            return Content(JsonConvert.SerializeObject(_torrentClient.GetTorrent()));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Notifications()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                _webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                while (_webSocket.State == WebSocketState.Open)
                {
                    var token = CancellationToken.None;
                    var buffer = new ArraySegment<byte>(new byte[4096]);
                    var received = await _webSocket.ReceiveAsync(buffer, token);
                    _log.LogInformation("recieved message from websocket");

                    switch (received.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            var request = new MyClass {message = "test"};
                            var type = WebSocketMessageType.Text;
                            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
                            buffer = new ArraySegment<byte>(data);
                            await _webSocket.SendAsync(buffer, type, true, token);
                            break;
                    }
                }
            }
            return Ok();
        }

        //private async void TorrentReportStats(object sender, StatsEventArgs e)
        //{
        //    total = e.TotalPeers;
        //    //if (_webSocket?.State == WebSocketState.Open)
        //    //{
        //    //    var token = CancellationToken.None;
        //    //    var buffer = new ArraySegment<byte>(new byte[4096]);

        //    //            var request = new MyClass() { message = total.ToString() };
        //    //            var type = WebSocketMessageType.Text;
        //    //            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
        //    //            buffer = new ArraySegment<byte>(data);
        //    //            await _webSocket.SendAsync(buffer, type, true, token);
        //    //}

        //    Console.WriteLine("Total peers: " + e.TotalPeers);
        //}

        //private void TorrentStateChanged(object sender, EventArgs<TorrentState> e)
        //{
        //    switch (e.Value)
        //    {
        //        case TorrentState.Seeding:
        //            _torrent.Stop();
        //            _torrent.StateChanged -= TorrentStateChanged;
        //            _log.Info("Starting ffmpeg");
        //            try
        //            {
        //                foreach (var file in _torrent.Data.Files)
        //                    if (file.IsMedia() && Path.GetExtension(file.Name) != ".mp4")
        //                    {
        //                        var fileToConvert = Directory.EnumerateFiles(_environment.ContentRootPath, file.Name,
        //                                SearchOption.AllDirectories)
        //                            .FirstOrDefault();

        //                        var processInfo = new ProcessStartInfo("/app/vendor/ffmpeg/ffmpeg")
        //                        {
        //                            Arguments = string.Format(@"-i {0} -f mp4 -vcodec libx264 -preset ultrafast 
        //                                                        -movflags faststart -profile:v main -acodec aac {1} -hide_banner",
        //                                fileToConvert,
        //                                string.Format("{0}.mp4", Path.ChangeExtension(fileToConvert, null)))
        //                        };

        //                        var process = Process.Start(processInfo);
        //                        process.Exited += Process_Exited;
        //                        //System.IO.File.Delete(fileToConvert);
        //                    }
        //            }
        //            catch (Exception exception)
        //            {
        //                _log.Error(exception);
        //            }

        //            break;
        //    }
        //}

        //private async void Process_Exited(object sender, EventArgs e)
        //{
        //    var process = (Process)sender;
        //    _log.Info(await process.StandardOutput.ReadToEndAsync());

        //    if (process.ExitCode == 0)
        //        _log.Info("file has been converted");
        //}

        public static string RemoveInvalidFilePathCharacters(string filename, string replaceChar)
        {
            var invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var regex = new Regex(string.Format("[{0}]", Regex.Escape(invalidChars)));
            return regex.Replace(filename, replaceChar);
        }
    }
}