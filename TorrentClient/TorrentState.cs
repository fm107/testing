namespace Torrent.Client
{
    public enum TorrentState
    {
        NotRunning,
        WaitingForTracker,
        WaitingForDisk,
        Downloading,
        Seeding,
        Finished,
        Hashing,
        Failed
    }
}