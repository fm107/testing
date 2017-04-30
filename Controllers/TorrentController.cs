using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeMapping;
using Newtonsoft.Json;
using WebTorrent.Extensions;


using System.Net.Sockets;
using System.Net.BitTorrent.Common;
using System.Net.BitTorrent.Client;
using System.Net;
using System.Net.BitTorrent.BEncoding;
using System.Net.BitTorrent.Client.Encryption;
using System.Net.BitTorrent.Client.Tracker;
using System.Net.BitTorrent.Dht;
using System.Net.BitTorrent.Dht.Listeners;
using System.Runtime.Loader;
using UTorrent.Api;


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
        //private TorrentTransfer _torrent;

        private WebSocket _webSocket;
        private int total;


        static string dhtNodeFile;
        static string basePath;
        static string downloadsPath;
        static string fastResumeFile;
        static string torrentsPath;
        static ClientEngine engine;				// The engine used for downloading
        static List<TorrentManager> torrents;	// The list where all the torrentManagers will be stored that the engine gives us
        static Top10Listener listener;			// This is a subclass of TraceListener which remembers the last 20 statements sent to it

        public TorrentController(IHostingEnvironment environment)
        {
            _environment = environment;
            _client = new HttpClient();
            _log = LogManager.GetLogger(Assembly.GetEntryAssembly(), "TorrentController");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UploadFromUrl([FromQuery] string url, [FromQuery] string folder)
        {
            var response = await _client.GetAsync(url, HttpCompletionOption.ResponseContentRead);

            var uploads = Path.Combine(_environment.WebRootPath, folder);

            _fileName = Path.Combine(uploads, RemoveInvalidFilePathCharacters(
                response.Content.Headers.ContentDisposition != null
                    ? response.Content.Headers.ContentDisposition.FileName.Trim('\u0022')
                    : response.RequestMessage.RequestUri.Segments.LastOrDefault(), "_"));

            using (var fileStream = new FileStream(_fileName, FileMode.Create))
            {
                await response.Content.CopyToAsync(fileStream);
            }

            if (MimeTypes.GetMimeMapping(_fileName) != "application/x-bittorrent")
            {
                return BadRequest("Not application/x-bittorrent Mime type");
            }

            _log.Info("Starting torrent manager");
            _log.InfoFormat("file path is {0}", _fileName);

            try
            {
                //_torrent = new TorrentTransfer(_fileName, uploads);
                //_torrent.StateChanged += TorrentStateChanged;
                //_torrent.ReportStats += TorrentReportStats;
                //_torrent.Start();

                UTorrentClient client = new UTorrentClient("admin", "");

                var response2 = client.PostTorrent(new FileStream(_fileName, FileMode.Open), @"test");
                var torrent = response2.AddedTorrent;

                var set = client.GetSettings().Result;
                var resp = client.GetList();
                var torrents = resp.Result.Torrents;

                foreach (var tor in torrents)
                {
                    Console.WriteLine(tor.Path);
                    Console.WriteLine(tor.Name);
                    Console.WriteLine(tor.Remaining);
                }
                
            }
            catch (Exception exception)
            {
                _log.Error(exception);
            }

            return Ok(Path.GetFileName(_fileName));
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
                //_torrent = new TorrentTransfer(_fileName, uploads);
                //_torrent.StateChanged += TorrentStateChanged;
                //_torrent.ReportStats += TorrentReportStats;
                //_torrent.Start();

                basePath = Directory.GetCurrentDirectory();//Environment.CurrentDirectory;						// This is the directory we are currently in
                torrentsPath = uploads;             // This is the directory we will save .torrents to
                downloadsPath = uploads;            // This is the directory we will save downloads to
                fastResumeFile = Path.Combine(torrentsPath, "fastresume.data");
                dhtNodeFile = Path.Combine(torrentsPath, "DhtNodes");
                torrents = new List<TorrentManager>();                          // This is where we will store the torrentmanagers
                listener = new Top10Listener(10);

                // We need to cleanup correctly when the user closes the window by using ctrl-c
                // or an unhandled exception happens
                Console.CancelKeyPress += delegate { shutdown(); };
                AssemblyLoadContext.Default.Unloading += Default_Unloading;

                StartEngine();
            }
            catch (Exception exception)
            {
                _log.Error(exception);
            }

            return Ok("{}");
        }

        private static void Default_Unloading(AssemblyLoadContext obj)
        {
            shutdown();
        }

        private static void StartEngine()
        {
            const int port = 64111;

            // Create the settings which the engine will use
            // downloadsPath - this is the path where we will save all the files to
            // port - this is the port we listen for connections on
            EngineSettings engineSettings = new EngineSettings(downloadsPath, port);
            engineSettings.PreferEncryption = false;
            engineSettings.AllowedEncryption = EncryptionTypes.All;
            

            // Create the default settings which a torrent will have.
            // 4 Upload slots - a good ratio is one slot per 5kB of upload speed
            // 50 open connections - should never really need to be changed
            // Unlimited download speed - valid range from 0 -> int.Max
            // Unlimited upload speed - valid range from 0 -> int.Max
            TorrentSettings torrentDefaults = new TorrentSettings(4, 150, 0, 0);

            // Create an instance of the engine.
            engine = new ClientEngine(engineSettings);
            engine.ChangeListenEndpoint(new IPEndPoint(IPAddress.Any, port));
            byte[] nodes = null;
            try
            {
                nodes = System.IO.File.ReadAllBytes(dhtNodeFile);
            }
            catch
            {
                Console.WriteLine("No existing dht nodes could be loaded");
            }

            DhtListener dhtListner = new DhtListener(new IPEndPoint(IPAddress.Any, port));
            DhtEngine dht = new DhtEngine(dhtListner);
            engine.RegisterDht(dht);
            dhtListner.Start();
            engine.DhtEngine.Start(nodes);

            // If the SavePath does not exist, we want to create it.
            if (!Directory.Exists(engine.Settings.SavePath))
                Directory.CreateDirectory(engine.Settings.SavePath);

            // If the torrentsPath does not exist, we want to create it
            if (!Directory.Exists(torrentsPath))
                Directory.CreateDirectory(torrentsPath);

            BEncodedDictionary fastResume;
            try
            {
                fastResume = BEncodedValue.Decode<BEncodedDictionary>(System.IO.File.ReadAllBytes(fastResumeFile));
            }
            catch
            {
                fastResume = new BEncodedDictionary();
            }

            // For each file in the torrents path that is a .torrent file, load it into the engine.
            foreach (string file in Directory.GetFiles(torrentsPath))
            {
                if (file.EndsWith(".torrent"))
                {
                    System.Net.BitTorrent.Common.Torrent torrent = null;
                    try
                    {
                        // Load the .torrent from the file into a Torrent instance
                        // You can use this to do preprocessing should you need to
                        torrent = System.Net.BitTorrent.Common.Torrent.Load(file);
                        Console.WriteLine(torrent.InfoHash.ToString());
                    }
                    catch (Exception e)
                    {
                        Console.Write("Couldn't decode {0}: ", file);
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    // When any preprocessing has been completed, you create a TorrentManager
                    // which you then register with the engine.
                    TorrentManager manager = new TorrentManager(torrent, downloadsPath, torrentDefaults);
                    if (fastResume.ContainsKey(torrent.InfoHash.ToHex()))
                        manager.LoadFastResume(new FastResume((BEncodedDictionary)fastResume[torrent.infoHash.ToHex()]));
                    engine.Register(manager);

                    // Store the torrent manager in our list so we can access it later
                    torrents.Add(manager);
                    manager.PeersFound += new EventHandler<PeersAddedEventArgs>(manager_PeersFound);
                }
            }

            // If we loaded no torrents, just exist. The user can put files in the torrents directory and start
            // the client again
            if (torrents.Count == 0)
            {
                Console.WriteLine("No torrents found in the Torrents directory");
                Console.WriteLine("Exiting...");
                engine.Dispose();
                return;
            }

            // For each torrent manager we loaded and stored in our list, hook into the events
            // in the torrent manager and start the engine.
            foreach (TorrentManager manager in torrents)
            {
                // Every time a piece is hashed, this is fired.
                manager.PieceHashed += delegate (object o, PieceHashedEventArgs e) {
                    lock (listener)
                        listener.WriteLine(string.Format("Piece Hashed: {0} - {1}", e.PieceIndex, e.HashPassed ? "Pass" : "Fail"));
                };

                // Every time the state changes (Stopped -> Seeding -> Downloading -> Hashing) this is fired
                manager.TorrentStateChanged += delegate (object o, TorrentStateChangedEventArgs e) {
                    lock (listener)
                        listener.WriteLine("OldState: " + e.OldState.ToString() + " NewState: " + e.NewState.ToString());
                };

                // Every time the tracker's state changes, this is fired
                foreach (TrackerTier tier in manager.TrackerManager)
                {
                    foreach (System.Net.BitTorrent.Client.Tracker.Tracker t in tier.Trackers)
                    {
                        t.AnnounceComplete += delegate (object sender, AnnounceResponseEventArgs e) {
                            listener.WriteLine(string.Format("{0}: {1}", e.Successful, e.Tracker.ToString()));
                        };
                    }
                }
                // Start the torrentmanager. The file will then hash (if required) and begin downloading/seeding
                manager.Start();
            }

            // While the torrents are still running, print out some stats to the screen.
            // Details for all the loaded torrent managers are shown.
            int i = 0;
            bool running = true;
            StringBuilder sb = new StringBuilder(1024);
            while (running)
            {
                if ((i++) % 10 == 0)
                {
                    sb.Remove(0, sb.Length);
                    running = torrents.Exists(delegate (TorrentManager m) { return m.State != TorrentState.Stopped; });

                    AppendFormat(sb, "Total Download Rate: {0:0.00}kB/sec", engine.TotalDownloadSpeed / 1024.0);
                    AppendFormat(sb, "Total Upload Rate:   {0:0.00}kB/sec", engine.TotalUploadSpeed / 1024.0);
                    AppendFormat(sb, "Disk Read Rate:      {0:0.00} kB/s", engine.DiskManager.ReadRate / 1024.0);
                    AppendFormat(sb, "Disk Write Rate:     {0:0.00} kB/s", engine.DiskManager.WriteRate / 1024.0);
                    AppendFormat(sb, "Total Read:         {0:0.00} kB", engine.DiskManager.TotalRead / 1024.0);
                    AppendFormat(sb, "Total Written:      {0:0.00} kB", engine.DiskManager.TotalWritten / 1024.0);
                    AppendFormat(sb, "Open Connections:    {0}", engine.ConnectionManager.OpenConnections);

                    foreach (TorrentManager manager in torrents)
                    {
                        AppendSeperator(sb);
                        AppendFormat(sb, "State:           {0}", manager.State);
                        AppendFormat(sb, "Name:            {0}", manager.Torrent == null ? "MetaDataMode" : manager.Torrent.Name);
                        AppendFormat(sb, "Progress:           {0:0.00}", manager.Progress);
                        AppendFormat(sb, "Download Speed:     {0:0.00} kB/s", manager.Monitor.DownloadSpeed / 1024.0);
                        AppendFormat(sb, "Upload Speed:       {0:0.00} kB/s", manager.Monitor.UploadSpeed / 1024.0);
                        AppendFormat(sb, "Total Downloaded:   {0:0.00} MB", manager.Monitor.DataBytesDownloaded / (1024.0 * 1024.0));
                        AppendFormat(sb, "Total Uploaded:     {0:0.00} MB", manager.Monitor.DataBytesUploaded / (1024.0 * 1024.0));
                        System.Net.BitTorrent.Client.Tracker.Tracker tracker = manager.TrackerManager.CurrentTracker;
                        //AppendFormat(sb, "Tracker Status:     {0}", tracker == null ? "<no tracker>" : tracker.State.ToString());
                        AppendFormat(sb, "Warning Message:    {0}", tracker == null ? "<no tracker>" : tracker.WarningMessage);
                        AppendFormat(sb, "Failure Message:    {0}", tracker == null ? "<no tracker>" : tracker.FailureMessage);
                        if (manager.PieceManager != null)
                            AppendFormat(sb, "Current Requests:   {0}", manager.PieceManager.CurrentRequestCount());

                        foreach (PeerId p in manager.GetPeers())
                            AppendFormat(sb, "\t{2} - {1:0.00}/{3:0.00}kB/sec - {0}", p.Peer.ConnectionUri,
                                p.Monitor.DownloadSpeed / 1024.0,
                                p.AmRequestingPiecesCount,
                                p.Monitor.UploadSpeed / 1024.0);

                        AppendFormat(sb, "", null);
                        if (manager.Torrent != null)
                            foreach (TorrentFile file in manager.Torrent.Files)
                                AppendFormat(sb, "{1:0.00}% - {0}", file.Path, file.BitField.PercentComplete);
                    }
                    Console.Clear();
                    Console.WriteLine(sb.ToString());
                    
                }

                System.Threading.Thread.Sleep(500);
            }
        }

        static void manager_PeersFound(object sender, PeersAddedEventArgs e)
        {
            lock (listener)
                listener.WriteLine(string.Format("Found {0} new peers and {1} existing peers", e.NewPeers, e.ExistingPeers));//throw new Exception("The method or operation is not implemented.");
        }

        private static void AppendSeperator(StringBuilder sb)
        {
            AppendFormat(sb, "", null);
            AppendFormat(sb, "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -", null);
            AppendFormat(sb, "", null);
        }
        private static void AppendFormat(StringBuilder sb, string str, params object[] formatting)
        {
            if (formatting != null)
                sb.AppendFormat(str, formatting);
            else
                sb.Append(str);
            sb.AppendLine();
        }

        private static void shutdown()
        {
            BEncodedDictionary fastResume = new BEncodedDictionary();
            for (int i = 0; i < torrents.Count; i++)
            {
                torrents[i].Stop(); ;
                while (torrents[i].State != TorrentState.Stopped)
                {
                    Console.WriteLine("{0} is {1}", torrents[i].Torrent.Name, torrents[i].State);
                    Thread.Sleep(250);
                }

                fastResume.Add(torrents[i].Torrent.InfoHash.ToHex(), torrents[i].SaveFastResume().Encode());
            }

#if !DISABLE_DHT
            System.IO.File.WriteAllBytes(dhtNodeFile, engine.DhtEngine.SaveNodes());
#endif
            System.IO.File.WriteAllBytes(fastResumeFile, fastResume.Encode());
            engine.Dispose();

            System.Threading.Thread.Sleep(2000);
        }

        public class Top10Listener
        {
            public Top10Listener(int i)
            {
                Console.WriteLine(i);
            }

            public void WriteLine(string format)
            {
                Console.WriteLine(format);
            }
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
                    _log.Info("recieved message from websocket");

                    switch (received.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            var request = new MyClass() { message = total.ToString() };
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