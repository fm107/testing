using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Torrent.Client.Events;
using Torrent.Client.Extensions;

namespace Torrent.Client
{
    public class BlockStrategist
    {
        private readonly int _blockCount;
        private readonly int _blockSize;
        private readonly int[] _pieces;
        private readonly int _pieceSize;
        private readonly long _totalSize;
        private readonly BlockAddressCollection<int> _unavailable;

        public BlockStrategist(TorrentData data)
        {
            _blockSize = Global.Instance.BlockSize;
            _pieceSize = data.PieceLength;
            _totalSize = data.Files.Sum(f => f.Length);
            _blockCount = (int) Math.Ceiling((float) _totalSize / _blockSize);
            _pieces = new int[data.PieceCount];
            Bitfield = new BitArray(data.PieceCount);
            _unavailable = new BlockAddressCollection<int>();
            for (var i = 0; i < _blockCount; i++)
                _unavailable.Add(i);

            for (var i = 0; i < data.PieceCount - 1; i++)
                _pieces[i] = data.PieceLength;
            var lastLength = (int) (data.TotalLength - data.PieceLength * (data.PieceCount - 1));
            _pieces[_pieces.Length - 1] = lastLength;
        }

        public BitArray Bitfield { get; }
        public int Available { get; private set; }

        public bool Complete
        {
            get
            {
                lock (_unavailable)
                {
                    return !_unavailable.Any();
                }
            }
        }

        public BlockInfo Next(BitArray bitfield)
        {
            if (Available == _blockCount)
                return BlockInfo.Empty;
            BlockInfo block;
            var counter = 0;
            do
            {
                //увеличаваме брояча на опитите за намиране на произволен блок
                counter++;
                int index;
                lock (_unavailable)
                {
                    //ако има липсващи блокове, избираме случаен от тях
                    if (_unavailable.Any())
                        index = _unavailable.Random();
                    else return BlockInfo.Empty; //в противен случай, връщаме празен
                }
                //преобразуване на адрес
                block = Block.FromAbsoluteAddress((long) index * _blockSize, _pieceSize, _blockSize,
                    _totalSize);
                if (counter > 10) //ако броячът за опитите надвиши 10, връщаме блока, независимо дали пиъра го има
                    return block;
                //повторяме докато не установим, че bitfield-а на пиъра съдържа произволно избрания блок
            } while (!bitfield[block.Index]);
            return block;
        }

        public bool Received(BlockInfo block)
        {
            //изчисляване на адреса на блока
            var address = (int) (Block.GetAbsoluteAddress(block.Index, block.Offset, _pieceSize) / _blockSize);
            lock (_unavailable)
            {
                //ако колекцията с липсващи блокове включва адреса на блока, и блока е с дължина
                //по-голяма от 0, тогава го обработваме
                if (_unavailable.Contains(address) && block.Length > 0)
                {
                    //премахване на новия блок от колекцията със липсващи
                    Debug.WriteLine("Needed block incoming:" + address);
                    _unavailable.Remove(address);
                    Available++; //увеличаване на броя на наличните блокове
                    _pieces[block.Index] -= block.Length;
                        //изваждане на размера на блока от размера на парчето, което го съдържа
                    if (_pieces[block.Index] <= 0)
                        //ако парчето, съдържащо блока, има размер 0, тогава обявяваме парчето за свалено
                        SetDownloaded(block.Index);
                    return true;
                }
                Debug.WriteLine("Unneeded block incoming:" + address);
                return false;
            }
        }

        private void SetDownloaded(int piece)
        {
            Bitfield.Set(piece, true);
            OnHavePiece(piece);
        }

        public event EventHandler<EventArgs<int>> HavePiece;

        private void OnHavePiece(int e)
        {
            HavePiece?.Invoke(this, new EventArgs<int>(e));
        }
    }

    public class BlockAddressCollection<T> : KeyedCollection<int, int>
    {
        protected override int GetKeyForItem(int item)
        {
            return item;
        }
    }
}