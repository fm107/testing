using System;
using System.Threading;
using Torrent.Client.Messages;

namespace Torrent.Client
{
    public class DownloadMode : TorrentMode
    {
        private const int RequestsQueueLength = 10;
        private const int MaxConnectedPeers = 100;
        private long _pendingWrites;

        public DownloadMode(HashingMode hashMode)
            : this(new BlockManager(hashMode.Metadata, hashMode.BlockManager.MainDirectory),
                hashMode.BlockStrategist, hashMode.Metadata, hashMode.Monitor)
        {
        }

        public DownloadMode(BlockManager manager, BlockStrategist strategist, TorrentData metadata,
            TransferMonitor monitor) :
            base(manager, strategist, metadata, monitor)
        {
            strategist.HavePiece += (sender, args) => SendHaveMessages(args.Value);
        }

        public override void Start()
        {
            base.Start();
            if (BlockStrategist.Complete)
            {
                OnDownloadComplete();
                OnFlushedToDisk();
                Stop(true);
                return;
            }

            PeerListener.Register(Metadata.InfoHash, peer => SendHandshake(peer, DefaultHandshake));
        }

        public override void Stop(bool closeStreams)
        {
            base.Stop(closeStreams);
            PeerListener.Deregister(Metadata.InfoHash);
        }

        protected override void HandleRequest(RequestMessage request, PeerState peer)
        {
            if (!peer.IsChoked && request.Length <= Global.Instance.BlockSize)
                BlockManager.GetBlock(new byte[request.Length], request.Index, request.Offset, request.Length, BlockRead,
                    peer);
        }

        protected override void HandlePiece(PieceMessage piece, PeerState peer)
        {
            var blockInfo = new BlockInfo(piece.Index, piece.Offset, piece.Data.Length);
            //съобщаваме на BlockStrategistът, че сме получили блок, а той ни казва дали ни е нужен
            if (BlockStrategist.Received(blockInfo))
                WriteBlock(piece);
            //понижаване на брояча за блоковете в изчакване
            peer.PendingBlocks--;
            //изпращане на нова заявка за блок към пиъра
            SendBlockRequests(peer);
        }

        protected override void HandleUnchoke(UnchokeMessage unchoke, PeerState peer)
        {
            base.HandleUnchoke(unchoke, peer);
            SendBlockRequests(peer);
        }

        protected override void HandleBitfield(BitfieldMessage bitfield, PeerState peer)
        {
            //първо викаме имплементацията в TorrentMode
            base.HandleBitfield(bitfield, peer);
            //ако пиъра няма никакви налични блокове, тогава не ни трябва   
            if (!peer.NoBlocks)
                SendMessage(peer, new InterestedMessage());
        }

        protected override void HandleInterested(InterestedMessage interested, PeerState peer)
        {
            base.HandleInterested(interested, peer);
            peer.IsChoked = false;
            SendMessage(peer, new UnchokeMessage());
        }

        protected override bool AddPeer(PeerState peer)
        {
            if (Peers.Count >= MaxConnectedPeers) return false;

            SendBitfield(peer);
            return base.AddPeer(peer);
        }

        private void SendBitfield(PeerState peer)
        {
            SendMessage(peer, new BitfieldMessage(BlockStrategist.Bitfield));
        }

        private void SendBlockRequests(PeerState peer)
        {
            //изчисляване на броя на нужните блокове
            var count = RequestsQueueLength - peer.PendingBlocks;
            for (var i = 0; i < count; i++)
            {
                //поискване на нов блок от BlockStrategistа
                var block = BlockStrategist.Next(peer.Bitfield);
                if (block != BlockInfo.Empty)
                {
                    //ако адреса на блока е валиден - тоест, има нужда от нов блок, изпраща се съобщение
                    SendMessage(peer, new RequestMessage(block.Index, block.Offset, block.Length));
                    //увеличаване на броя на изчакващите блокове
                    peer.PendingBlocks++;
                }
                else if (BlockStrategist.Complete)
                {
                    //ако адреса на блока е невалиден, и всички блокове са свалени
                    //сигнализираме приключено изтегляне
                    OnDownloadComplete();
                    return;
                }
            }
        }

        private void SendHaveMessages(int piece)
        {
            foreach (var peer in Peers.Values)
                if (!peer.Bitfield[piece])
                    SendMessage(peer, new HaveMessage(piece));
        }

        private void WriteBlock(PieceMessage piece)
        {
            try
            {
                var block = new Block(piece.Data, piece.Index, piece.Offset, piece.Data.Length);
                BlockManager.AddBlock(block, BlockWritten, block);
                Interlocked.Add(ref _pendingWrites, piece.Data.Length);
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        private void BlockWritten(bool success, object state)
        {
            var block = (Block) state;
            if (success)
            {
                Monitor.Written(block.Info.Length);
                Interlocked.Add(ref _pendingWrites, -block.Info.Length);
            }
            if (BlockStrategist.Complete && _pendingWrites == 0)
                AllWrittenToDisk();
        }

        private void BlockRead(bool success, Block block, object state)
        {
            var peer = (PeerState) state;
            try
            {
                if (success)
                {
                    Monitor.Read(block.Info.Length);
                    SendMessage(peer, new PieceMessage(block.Info.Index, block.Info.Offset, block.Data));
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        private void AllWrittenToDisk()
        {
            Stop(true);
            OnFlushedToDisk();
        }

        private void OnDownloadComplete()
        {
            DownloadComplete?.Invoke(this, new EventArgs());
        }

        private void OnFlushedToDisk()
        {
            FlushedToDisk?.Invoke(this, new EventArgs());
        }

        public event EventHandler DownloadComplete;
        public event EventHandler FlushedToDisk;
    }
}