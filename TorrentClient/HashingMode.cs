using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Torrent.Client.Messages;

namespace Torrent.Client
{
    public class HashingMode : TorrentMode
    {
        private readonly SHA1 _hasher = SHA1.Create();
        private byte[] _pieceBuffer;
        private int _remainingPieces;

        public HashingMode(BlockManager manager, BlockStrategist strategist, TorrentData metadata, TransferMonitor monitor) :
            base(manager, strategist, metadata, monitor)
        {
        }

        public override void Start()
        {
            base.Start();
            Task.Factory.StartNew(StartTask);
        }

        private void StartTask()
        {
            if (!Directory.Exists(BlockManager.MainDirectory))
            {
                Stop(true);
                OnHashingComplete();
                return;
            }
            _pieceBuffer = new byte[Metadata.PieceLength];
            _remainingPieces = Metadata.PieceCount;
            var lastPieceLength = (int) (Metadata.TotalLength - Metadata.PieceLength * (Metadata.PieceCount - 1));
            //цикъл за проверка всеки блок (без последния, който може да бъде с произволен размер)
            for (var i = 0; i < Metadata.PieceCount - 1; i++)
            {
                if (Stopping) return;
                try
                {
                    //асинхронна заявка за прочитане на блок от файловата система
                    BlockManager.GetBlock(_pieceBuffer, i, 0, Metadata.PieceLength, PieceRead, i);
                }
                catch
                {
                    // ignored
                }
            }
            try
            {
                //обработка на последния блок
                BlockManager.GetBlock(_pieceBuffer, Metadata.PieceCount - 1, 0, lastPieceLength, PieceRead,
                    Metadata.PieceCount - 1);
            }
            catch
            {
                // ignored
            }
        }

        private void PieceRead(bool success, Block block, object state)
        {
            if (Stopping) return;
            Interlocked.Decrement(ref _remainingPieces); //безопасно декрементиране на брояча
            var piece = (int) state;
            if (success)
                lock (block.Data)
                {
                    if (HashCheck(block))
                        MarkAvailable(piece);
                }
            //ако остават 0 парчета за проверяване, процесът е завършил
            //спиране и съобщение за приключване
            if (_remainingPieces == 0)
            {
                Stop(true);
                OnHashingComplete();
            }
        }

        private bool HashCheck(Block block)
        {
            //изчисляване на хеш стойността на блока
            var hash = _hasher.ComputeHash(block.Data, 0, block.Info.Length);
            //сверяване на получения хеш код със този, указан в метаданните
            return hash.SequenceEqual(Metadata.Checksums[block.Info.Index]);
        }

        private void MarkAvailable(int piece)
        {
            var blocksPerPiece = Metadata.PieceLength / Global.Instance.BlockSize;
            var blockSize = Global.Instance.BlockSize;
            for (var i = 0; i < blocksPerPiece; i++)
                BlockStrategist.Received(new BlockInfo(piece, blockSize * i, blockSize));
        }

        protected override void HandleException(Exception e)
        {
            Stop(true);
            OnHashingComplete();
        }

        protected override void HandleRequest(RequestMessage request, PeerState peer)
        {
        }

        protected override void HandlePiece(PieceMessage piece, PeerState peer)
        {
        }

        public event EventHandler HashingComplete;

        private void OnHashingComplete()
        {
            HashingComplete?.Invoke(this, new EventArgs());
        }
    }
}