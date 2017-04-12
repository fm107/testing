using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Torrent.Client;
using Torrent.Client.Events;

namespace WebTorrent.Controllers
{
    public class myclass
    {
        private const string Path = @"D:\Temp\Torrents\tor.torrent";
        private TorrentTransfer _torrent;

        public void Run()
        {
            try
            {
                _torrent = new TorrentTransfer(Path, @"D:\Temp\Torrents");
                _torrent.StateChanged += Torrent_StateChanged;
                _torrent.ReportStats += TorrentOnReportStats;
                _torrent.Start();
            }
            finally
            {
                Console.Read();
            }
        }

        private void TorrentOnReportStats(object sender, StatsEventArgs statsEventArgs)
        {
           
        }

        private void _torrent_ReportStats(object sender, StatsEventArgs e)
        {
            //Console.WriteLine(e.BytesCompleted);
            //Console.WriteLine(e.TotalPeers);
            //Console.WriteLine(e.ChokedBy);
        }

        private void Torrent_StateChanged(object sender, EventArgs<TorrentState> e)
        {
            Console.WriteLine(e.Value);
        }
    }
}