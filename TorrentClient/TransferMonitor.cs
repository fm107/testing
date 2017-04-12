using System.Threading;

namespace Torrent.Client
{
    public class TransferMonitor
    {
        private long _bytesRead;
        private long _bytesReceived;
        private long _bytesSent;
        private long _bytesWritten;

        public TransferMonitor(InfoHash hash, long totalBytes)
        {
            TorrentHash = hash;
            TotalBytes = totalBytes;
        }

        public InfoHash TorrentHash { get; private set; }

        public long BytesReceived
        {
            get { return _bytesReceived; }
        }

        public long BytesSent
        {
            get { return _bytesSent; }
        }

        public long BytesWritten
        {
            get { return _bytesWritten; }
        }

        public long BytesRead
        {
            get { return _bytesRead; }
        }

        public long TotalBytes { get; private set; }

        public void Received(int count)
        {
            Interlocked.Add(ref _bytesReceived, count);
        }

        public void Sent(int count)
        {
            Interlocked.Add(ref _bytesSent, count);
        }

        public void Written(int count)
        {
            Interlocked.Add(ref _bytesWritten, count);
        }

        public void Read(int count)
        {
            Interlocked.Add(ref _bytesRead, count);
        }
    }
}