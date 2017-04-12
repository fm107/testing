using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Torrent.Client.Events;

namespace Torrent.Client
{
    public delegate void BlockReadDelegate(bool success, Block block, object state);

    public delegate void BlockWrittenDelegate(bool success, object state);

    public class BlockManager : IDisposable
    {
        public readonly int BlockSize = Global.Instance.BlockSize;
        private readonly int _blocksPerPiece;
        private readonly HashSet<string> _nonexistingFiles = new HashSet<string>();
        private readonly ConcurrentDictionary<string, FileStream> _openStreams;

        private readonly int _pieceSize;
        private readonly Cache<BlockReadState> _readCache;
        private readonly TorrentData _torrentData;
        private readonly Cache<BlockWriteState> _writeCache;
        private bool _disposed;
        private readonly int _queuedWrites = 0;
        private List<BlockReadState> _reads = new List<BlockReadState>();


        public BlockManager(TorrentData data, string mainDir)
        {
            _blocksPerPiece = (int) Math.Ceiling((double) data.PieceLength / BlockSize);
            _readCache = new Cache<BlockReadState>();
            _writeCache = new Cache<BlockWriteState>();
            _openStreams = new ConcurrentDictionary<string, FileStream>();
            _torrentData = data;
            _pieceSize = data.PieceLength;
            MainDirectory = mainDir;
        }

        public string MainDirectory { get; }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void AddBlock(Block block, BlockWrittenDelegate callback, object state)
        {
            try
            {
                var parts = GetParts(block.Info.Index, block.Info.Offset, block.Info.Length, true);
                var totalLen = parts.Sum(p => p.Length);
                var data = _writeCache.Get().Init(callback, (int) totalLen, block, state);
                //Console.WriteLine(parts.Any());
                foreach (var part in parts)
                    DiskIo.QueueWrite(part.FileStream, block.Data, part.FileOffset, part.DataOffset, part.Length,
                        EndAddBlock, data);
            }
            catch (Exception e)
            {
                OnRaisedException(new TorrentException(e.Message, e));
                callback(false, state);
            }
        }

        public void GetBlock(byte[] buffer, int pieceIndex, int offset, int length, BlockReadDelegate callback,
            object state)
        {
            try
            {
                var parts = GetParts(pieceIndex, offset, length, false);
                var block = new Block(buffer, pieceIndex, offset, length);
                var data = _readCache.Get().Init(block, callback, length, state);
                if (parts.Count() > 1)
                    Console.WriteLine(pieceIndex + " is split between " + parts.Count() + " files.");

                foreach (var part in parts)
                    if (part.FileStream != null)
                    {
                        DiskIo.QueueRead(part.FileStream, buffer, part.DataOffset, part.FileOffset, part.Length,
                            EndGetBlock, data);
                    }
                    else
                    {
                        OnRaisedException(new TorrentException("Stream is null."));
                        callback(false, null, state);
                        return;
                    }
            }
            catch (Exception e)
            {
                OnRaisedException(new TorrentException(e.Message, e));
                callback(false, null, state);
            }
        }

        private void EndAddBlock(bool success, int written, object state)
        {
            var data = (BlockWriteState) state;
            lock (state)
            {
                if (success)
                {
                    if (data.SubstractRemaining(written) <= 0)
                    {
                        data.Callback(true, data.State);
                        _writeCache.Put(data);
                    }
                }
                else
                {
                    data.Callback(false, data.State);
                    _writeCache.Put(data);
                }
            }
        }

        private void EndGetBlock(bool success, int read, byte[] pieceData, object state)
        {
            var data = (BlockReadState) state;

            if (success)
            {
                if (data.SubstractRemaining(read) <= 0)
                {
                    data.Callback(true, data.Block, data.State);
                    _readCache.Put(data);
                }
            }
            else
            {
                data.Callback(false, null, data.State);
                _readCache.Put(data);
            }
        }

        private IEnumerable<BlockPartInfo> GetParts(int pieceIndex, int offset, int length, bool write)
        {
            var pieces = new List<BlockPartInfo>();
            var requestedOffset = Block.GetAbsoluteAddress(pieceIndex, offset, _pieceSize);
            long currentOffset = 0;
            long remaining = length;
            foreach (var file in _torrentData.Files)
            {
                if (remaining <= 0) break;
                if (currentOffset + file.Length >= requestedOffset)
                {
                    var relativePosition = requestedOffset - currentOffset;
                    var partLength = Math.Min(file.Length - relativePosition, remaining);
                    var stream = GetStream(file, write);
                    if (stream == null)
                        throw new IOException("Stream is null.");
                    pieces.Add(new BlockPartInfo
                    {
                        FileStream = stream,
                        FileOffset = relativePosition,
                        Length = partLength,
                        DataOffset = length - (int) remaining
                    });
                    remaining -= partLength;
                    requestedOffset += partLength;
                }
                currentOffset += file.Length;
            }
            return pieces.ToArray();
        }

        private FileStream GetStream(FileEntry file, bool write)
        {
            const int tryCount = 5;
            var tryTime = 0;

            while (true)
                lock (_openStreams)
                {
                    var finalPath = Path.Combine(MainDirectory, file.Name);
                    if (!write && !FileExists(finalPath)) return null;

                    FileStream stream;
                    if (_openStreams.TryGetValue(finalPath, out stream))
                    {
                        if (!write) return stream;
                        if (stream.CanWrite) return stream;
                        _openStreams.TryRemove(finalPath, out stream);
                    }

                    try
                    {
                        var dir = Path.GetDirectoryName(finalPath);
                        if (dir != string.Empty && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        stream = File.Open(finalPath, write ? FileMode.OpenOrCreate : FileMode.Open,
                            write ? FileAccess.ReadWrite : FileAccess.Read);
                        _openStreams.TryAdd(finalPath, stream);
                        return stream;
                    }
                    catch (Exception)
                    {
                        tryTime++;
                        if (tryTime > tryCount)
                            throw;
                    }
                }
        }

        private bool FileExists(string finalPath)
        {
            if (_nonexistingFiles.Contains(finalPath)) return false;
            if (File.Exists(finalPath))
                return true;
            _nonexistingFiles.Add(finalPath);
            return false;
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    if (_openStreams != null)
                        foreach (var stream in _openStreams.Values)
                            if (stream != null)
                            {
                                stream.Flush();
                                stream.Dispose();
                            }
                _disposed = true;
            }
        }

        private void Wait(int count)
        {
            while (_queuedWrites > count)
                Thread.Sleep(10);
        }

        public event EventHandler<EventArgs<Exception>> RaisedException;

        public void OnRaisedException(Exception e)
        {
            RaisedException?.Invoke(this, new EventArgs<Exception>(e));
        }

        #region Nested type: BlockPartInfo

        public struct BlockPartInfo
        {
            public FileStream FileStream { get; set; }
            public long FileOffset { get; set; }
            public int DataOffset { get; set; }
            public long Length { get; set; }
        }

        #endregion

        #region Nested type: BlockReadState

        public class BlockReadState : ICacheable
        {
            private int _remaining;
            public Block Block { get; private set; }
            public BlockReadDelegate Callback { get; private set; }
            public object State { get; private set; }

            public int Remaining
            {
                get { return _remaining; }
                set { _remaining = value; }
            }

            #region ICacheable Members

            public ICacheable Init()
            {
                return Init(null, null, 0, null);
            }

            #endregion

            public BlockReadState Init(Block block, BlockReadDelegate callback, int remaining, object state)
            {
                Block = block;
                Callback = callback;
                State = state;
                Remaining = remaining;
                return this;
            }

            public int SubstractRemaining(int number)
            {
                return Interlocked.Add(ref _remaining, -number);
            }
        }

        #endregion

        #region Nested type: BlockWriteState

        public class BlockWriteState : ICacheable
        {
            private int _remaining;
            public BlockWrittenDelegate Callback { get; private set; }
            public object State { get; private set; }

            public int Remaining
            {
                get { return _remaining; }
                set { _remaining = value; }
            }

            public Block Block { get; private set; }

            #region ICacheable Members

            public ICacheable Init()
            {
                return Init(null, 0, null, null);
            }

            #endregion

            public BlockWriteState Init(BlockWrittenDelegate callback, int remaining, Block block, object state)
            {
                Callback = callback;
                State = state;
                Remaining = remaining;
                Block = block;
                return this;
            }

            public int SubstractRemaining(int number)
            {
                return Interlocked.Add(ref _remaining, -number);
            }
        }

        #endregion
    }
}