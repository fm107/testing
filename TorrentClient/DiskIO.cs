using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Torrent.Client
{
    public delegate void DiskIoReadCallback(bool success, int read, byte[] data, object state);

    public delegate void DiskIoWriteCallback(bool success, int written, object state);

    internal static class DiskIo
    {
        private const int MaxPendingOps = 2500;
        private const int MinPendingOps = 1000;

        private static readonly ConcurrentQueue<DiskIoReadState> ReadQueue;
        private static readonly ConcurrentQueue<DiskIoWriteState> WriteQueue;

        private static readonly Cache<DiskIoReadState> ReadCache;
        private static readonly Cache<DiskIoWriteState> WriteCache;

        private static readonly AutoResetEvent IoHandle;

        static DiskIo()
        {
            ReadQueue = new ConcurrentQueue<DiskIoReadState>();
            WriteQueue = new ConcurrentQueue<DiskIoWriteState>();
            ReadCache = new Cache<DiskIoReadState>();
            WriteCache = new Cache<DiskIoWriteState>();
            IoHandle = new AutoResetEvent(false);
            StartDiskThread();
        }


        public static void QueueRead(Stream stream, byte[] buffer, int bufferOffset, long streamOffset, long length,
            DiskIoReadCallback callback, object state)
        {
            while (ReadQueue.Count > MaxPendingOps)
                Thread.Sleep(10);

            var readData = ReadCache.Get().Init(stream, buffer, bufferOffset, streamOffset, length, callback,
                state);
            ReadQueue.Enqueue(readData);
            if (ReadQueue.Count > MinPendingOps) IoHandle.Set();
        }

        public static void QueueWrite(Stream stream, byte[] data, long fileOffset, int dataOffset, long length,
            DiskIoWriteCallback callback,
            object state)
        {
            while (WriteQueue.Count > MaxPendingOps)
                Thread.Sleep(10);

            var writeData = WriteCache.Get().Init(stream, data, fileOffset, dataOffset, length, callback, state);
            WriteQueue.Enqueue(writeData);
            if (WriteQueue.Count > MinPendingOps) IoHandle.Set();
        }

        private static void StartDiskThread()
        {
            var diskThread = new Thread(DiskLoop);
            diskThread.IsBackground = true;
            diskThread.Start();
        }

        private static void DiskLoop()
        {
            while (true)
                try
                {
                    IoHandle.WaitOne(200);
                    bool write = true, read = true;
                    var readList = new List<DiskIoReadState>();
                    var writeList = new List<DiskIoWriteState>();
                    while (write)
                    {
                        DiskIoWriteState result;
                        write = WriteQueue.TryDequeue(out result);
                        if (write) writeList.Add(result);
                    }
                    while (read)
                    {
                        DiskIoReadState result;
                        read = ReadQueue.TryDequeue(out result);
                        if (read) readList.Add(result);
                    }

                    foreach (var readState in readList)
                        Read(readState);

                    foreach (var writeState in writeList)
                        Write(writeState);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
        }

        private static void Write(DiskIoWriteState state)
        {
            try
            {
                lock (state.Data)
                {
                    if (!state.Stream.CanWrite) throw new TorrentException("Stream unwritable.");

                    state.Stream.Seek(state.FileOffset, SeekOrigin.Begin);
                    state.Stream.Write(state.Data, state.DataOffset, (int) state.Length);
                    state.Callback(true, (int) state.Length, state.State);
                    WriteCache.Put(state);
                }
            }
            catch (Exception)
            {
                state.Callback(false, 0, state.State);
                WriteCache.Put(state);
            }
        }

        private static void Read(DiskIoReadState state)
        {
            try
            {
                lock (state.Buffer)
                {
                    if (!state.Stream.CanRead) throw new TorrentException("Stream unreadable.");
                    state.Stream.Seek(state.StreamOffset, SeekOrigin.Begin);
                    var read = state.Stream.Read(state.Buffer, state.BufferOffset, (int) state.Length);
                    if (read != state.Length)
                        state.Callback(false, read, null, state.State);
                    else state.Callback(true, read, state.Buffer, state.State);
                    ReadCache.Put(state);
                }
            }
            catch (Exception)
            {
                state.Callback(false, 0, null, state.State);
                ReadCache.Put(state);
            }
        }

        #region Nested type: DiskIOReadState

        private class DiskIoReadState : ICacheable
        {
            public Stream Stream { get; private set; }
            public byte[] Buffer { get; private set; }
            public int BufferOffset { get; private set; }
            public long StreamOffset { get; private set; }
            public long Length { get; private set; }
            public DiskIoReadCallback Callback { get; private set; }
            public object State { get; private set; }

            #region ICacheable Members

            public ICacheable Init()
            {
                return Init(null, null, 0, 0, 0, null, null);
            }

            #endregion

            public DiskIoReadState Init(Stream stream, byte[] buffer, int bufferOffset, long streamOffset, long length,
                DiskIoReadCallback callback, object state)
            {
                Stream = stream;
                BufferOffset = bufferOffset;
                StreamOffset = streamOffset;
                Length = length;
                Callback = callback;
                State = state;
                Buffer = buffer;
                return this;
            }
        }

        #endregion

        #region Nested type: DiskIOWriteState

        private class DiskIoWriteState : ICacheable
        {
            public Stream Stream { get; private set; }
            public byte[] Data { get; private set; }
            public long FileOffset { get; private set; }
            public int DataOffset { get; private set; }
            public long Length { get; private set; }
            public DiskIoWriteCallback Callback { get; private set; }
            public object State { get; private set; }

            #region ICacheable Members

            public ICacheable Init()
            {
                return Init(null, null, 0, 0, 0, null, null);
            }

            #endregion

            public DiskIoWriteState Init(Stream stream, byte[] data, long fileOffset, int dataOffset, long length,
                DiskIoWriteCallback callback, object state)
            {
                Stream = stream;
                FileOffset = fileOffset;
                DataOffset = dataOffset;
                Length = length;
                Data = data;
                Callback = callback;
                State = state;
                return this;
            }
        }

        #endregion
    }
}