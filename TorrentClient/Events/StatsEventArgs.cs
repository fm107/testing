using System;

namespace Torrent.Client.Events
{
    public class StatsEventArgs : EventArgs
    {
        public StatsEventArgs(long downloadedBytes, int totalPeers, int chokedBy, int queued)
        {
            BytesCompleted = downloadedBytes;
            TotalPeers = totalPeers;
            ChokedBy = chokedBy;
            QueuedRequests = queued;
        }

        public long BytesCompleted { get; private set; }
        public int TotalPeers { get; private set; }
        public int ChokedBy { get; private set; }
        public int QueuedRequests { get; private set; }
    }
}