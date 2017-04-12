﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using Torrent.Client.Events;

namespace Torrent.Client
{
    /// <summary>
    ///     Represents a BitTorrent data transfer.
    /// </summary>
    public class TorrentTransfer
    {
        private TransferMonitor _monitor;
        private Timer _statsReportTimer;
        private volatile bool _stop;
        private bool _stopping;
        private TrackerClient _tracker;
        private TrackerResponse _trackerData;

        /// <summary>
        ///     Initialize a torrent transfer with metadata from a file on the filesystem.
        /// </summary>
        /// <param name="torrentPath">Path to the torrent file.</param>
        public TorrentTransfer(string torrentPath, string downloadPath) : this(File.OpenRead(torrentPath), downloadPath)
        {
        }

        /// <summary>
        ///     Initialize a torrent transfer with metadata read from the specified stream.
        /// </summary>
        /// <param name="torrentStream">The stream to read the torrent metadata from.</param>
        public TorrentTransfer(Stream torrentStream, string downloadPath)
        {
            using (torrentStream) //разкодиране на метаданните
            using (var reader = new BinaryReader(torrentStream))
            {
                var bytes = reader.ReadBytes((int) reader.BaseStream.Length);
                Data = new TorrentData(bytes);
            }
            //създаване на класове за комуникация с тракера,
            //отчитане на състояние
            DownloadFolder = Path.Combine(downloadPath, Data.Name);
        }

        public bool Complete
        {
            get { return Mode != null ? Mode.BlockStrategist.Complete : false; }
        }

        public string DownloadFolder { get; }

        public AnnounceManager AnnounceManager { get; private set; }

        public TorrentMode Mode { get; private set; }

        /// <summary>
        ///     The metadata describing the torrent.
        /// </summary>
        public TorrentData Data { get; }

        public bool Running { get; private set; }

        public TorrentState State { get; private set; }

        /// <summary>
        ///     Starts the torrent transfer on a new thread.
        /// </summary>
        public void Start()
        {
            if (State != TorrentState.NotRunning) throw new TorrentException("Already started.");

            var torrentThread = new Thread(StartThread) {IsBackground = true};

            torrentThread.Start();
        }

        /// <summary>
        ///     Stops all torrent activity and shuts down the thread.
        /// </summary>
        public void Stop()
        {
            _stopping = true;
            AnnounceManager.Stopped();
        }


        private void StartThread()
        {
            StartActions();
            try
            {
                StartTransfer();
                WaitForStop();
            }
            catch (Exception e)
            {
                OnRaisedException(e);
            }
            StopActions();
        }

        private void StartTransfer()
        {
            EnterHashingMode();
        }

        private void EnterHashingMode()
        {
            ChangeState(TorrentState.Hashing);
            var hashingMode = new HashingMode(new BlockManager(Data, DownloadFolder),
                new BlockStrategist(Data), Data, _monitor);
            hashingMode.RaisedException += (s, e) => OnRaisedException(e.Value);
            hashingMode.HashingComplete += (sender, args) => HashingComplete();
            Mode = hashingMode;
            _statsReportTimer.Change(0, 250);
            Mode.Start();
        }

        private void HashingComplete()
        {
            if (!Mode.BlockStrategist.Complete)
                EnterDownloadMode();
            else
                EnterSeedMode();
        }

        private void EnterSeedMode()
        {
            ChangeState(TorrentState.Seeding);
            var mode = new SeedMode(Mode);
            mode.RaisedException += (s, e) => OnRaisedException(e.Value);
            mode.Start();
            Mode = mode;
            AnnounceManager.Started();
        }

        private void EnterDownloadMode()
        {
            //промяна на състоянието - информира потребителския интерфейс
            ChangeState(TorrentState.Downloading);
            var mode = new DownloadMode((HashingMode) Mode);
            //прикачане на обработваш код към събитията за получено изключение,
            //завършено изтегляне, и приключило записване на диска
            mode.RaisedException += (s, e) => OnRaisedException(e.Value);
            mode.FlushedToDisk += (s, e) => FlushedToDisk();
            mode.DownloadComplete += (s, e) => DownloadCompleted();
            //стартиране на режима
            mode.Start();
            Mode = mode;
            //информиране на тракера
            AnnounceManager.Started();
        }

        private void FlushedToDisk()
        {
            EnterHashingMode();
        }

        private void DownloadCompleted()
        {
            ChangeState(TorrentState.WaitingForDisk);
            AnnounceManager.Completed();
        }

        private void StartActions()
        {
            _stop = false;
            _stopping = false;
            Running = true;
            _tracker = new TrackerClient(Data.Announces);
            _statsReportTimer = new Timer(o => OnStatsReport(), new object(), 0, Timeout.Infinite);
                //таймер за отчитане на състоянието
            _monitor = new TransferMonitor(Data.InfoHash, Data.TotalLength);
            AnnounceManager = new AnnounceManager(Data.Announces, _monitor, Data);
            AnnounceManager.PeersReceived += (sender, args) =>
            {
                Mode?.AddEndpoints(args.Value);
            };
        }

        private void StopActions()
        {
            //последно съобщаване за състоянието
            OnStatsReport();
            //информиране на интерфейса, че трансферът е спрян
            ChangeState(TorrentState.NotRunning);
            //освобождаване на таймера за състояние
            _statsReportTimer.Dispose();
            //съобщаване на тракера, че трансферът е спрян
            AnnounceManager.Stopped();
            //освобождаване на обекта, управляващ тракер комуникацията
            AnnounceManager.Dispose();
            //спиране на текущо изпълняващия се режим
            Mode?.Stop(true);
            _stop = true;
            Running = false;
        }

        private void WaitForStop()
        {
            while (true)
            {
                Thread.Sleep(100);
                if (_stopping) break;
            }
        }

        private void ChangeState(TorrentState state)
        {
            if (State != state)
            {
                State = state;
                OnStateChanged(state);
            }
        }

        #region Events

        private void OnRaisedException(Exception e)
        {
            Stop();
            RaisedException?.Invoke(this, new EventArgs<Exception>(e));
        }

        /// <summary>
        ///     Fires when an exception occurs in the transfer thread.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> RaisedException;

        public event EventHandler<StatsEventArgs> ReportStats;

        private void OnStatsReport()
        {
            if (Mode == null) return;

            var downloaded = Math.Min(Mode.BlockStrategist.Available * Global.Instance.BlockSize, Data.TotalLength);
            var totalPeers = Mode.Peers.Count;
            var chokedBy = Mode.Peers.Count(p => p.Value.AmChoked);

            var stats = new StatsEventArgs(downloaded, totalPeers, chokedBy, 0);
            ReportStats?.Invoke(this, stats);
        }

        public event EventHandler<EventArgs<TorrentState>> StateChanged;

        private void OnStateChanged(TorrentState e)
        {
            StateChanged?.Invoke(this, new EventArgs<TorrentState>(e));
        }

        #endregion
    }
}