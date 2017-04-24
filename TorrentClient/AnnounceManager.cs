using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Torrent.Client.Events;

namespace Torrent.Client
{
    public class AnnounceManager : IDisposable
    {
        private bool _disposed;
        private readonly Timer _regularTimer;
        private readonly List<TrackerInfo> _trackers;

        public AnnounceManager(IEnumerable<string> announceUrls, TransferMonitor monitor, TorrentData data)
        {
            //иницализация на списъка с тракери, състоящ се от класове TrackerInfo, които осигуряват
            //връзка с отделни тракери и заявки до тях
            _trackers = new List<TrackerInfo>();
            //прибавяне на HTTP тракерите от подадения списък с адреси
            //_trackers.AddRange(announceUrls.Where(u => u.StartsWith("http")).Select(u => new TrackerInfo(u)));
            _trackers.AddRange(announceUrls.Select(u => new TrackerInfo(u)));
            //прикачане на събитието PeersReceived към всеки от тракерите (TrackerInfo)
            _trackers.ForEach(t => t.PeersReceived += (sender, args) => OnPeersReceived(args.Value));
            Monitor = monitor;
            Data = data;
            //инициализация на таймер за проверка на състоянието на всеки тракер
            _regularTimer = new Timer(Regular, new object(), 0, Timeout.Infinite);
            _regularTimer.Change(1000, 5000);
        }

        public IEnumerable<TrackerInfo> Trackers
        {
            get { return _trackers.AsEnumerable(); }
        }

        public TransferMonitor Monitor { get; }
        public TorrentData Data { get; }

        public void Dispose()
        {
            if (!_disposed)
            {
                _regularTimer.Dispose();
                _disposed = true;
            }
        }

        public void Started()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var tracker in Trackers)
                    tracker.Started(Data.InfoHash, Monitor.BytesReceived,
                        Monitor.BytesSent, Monitor.TotalBytes - Monitor.BytesReceived);
            });
        }

        public void Completed()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var tracker in Trackers)
                    tracker.Completed(Data.InfoHash, Monitor.BytesReceived,
                        Monitor.BytesSent, Monitor.TotalBytes - Monitor.BytesReceived);
            });
        }

        public void Stopped()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var tracker in Trackers)
                    tracker.Stopped(Data.InfoHash, Monitor.BytesReceived,
                        Monitor.BytesSent, Monitor.TotalBytes - Monitor.BytesReceived);
            });
        }

        private void Regular(object o)
        {
            //стартиране на нова асинхронна задача
            Task.Factory.StartNew(() =>
            {
                //итериране през всички налични тракери
                foreach (var tracker in Trackers)
                    if (tracker.LastAnnounced.Add(tracker.Period) < DateTime.Now &&
                        tracker.LastState != AnnounceState.None)
                        if (tracker.LastState != AnnounceState.StartFailure)
                            tracker.Regular(Data.InfoHash, Monitor.BytesReceived,
                                Monitor.BytesSent, Monitor.TotalBytes - Monitor.BytesReceived);
                        else //в противен случай, повтаряме заявката Started, докато тя не стане успешна
                            tracker.Started(Data.InfoHash, Monitor.BytesReceived,
                                Monitor.BytesSent, Monitor.TotalBytes - Monitor.BytesReceived);
            });
        }

        public event EventHandler<EventArgs<IEnumerable<IPEndPoint>>> PeersReceived;

        public void OnPeersReceived(IEnumerable<IPEndPoint> e)
        {
            if (_disposed) return;
            PeersReceived?.Invoke(this, new EventArgs<IEnumerable<IPEndPoint>>(e));
        }
    }
}