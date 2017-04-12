using System;
using System.Diagnostics;

namespace Torrent.Client
{
    public class Block
    {
        public Block(byte[] data, int pieceIndex, int offset, int length)
        {
            Data = data;
            Info = new BlockInfo(pieceIndex, offset, length);
        }

        public byte[] Data { get; private set; }
        public BlockInfo Info { get; private set; }

        public static BlockInfo FromAbsoluteAddress(long byteOffset, int pieceSize, int length,
            long maxAbsoluteOffset = 0)
        {
            if (maxAbsoluteOffset == 0) maxAbsoluteOffset = byteOffset + length;
            long offset;
            var block = (int) DivRem(byteOffset, pieceSize, out offset);
            length = (int) Math.Min(maxAbsoluteOffset - byteOffset, length);
            Debug.Assert(block >= 0);
            return new BlockInfo(block, (int) offset, length);
        }

        public static long GetAbsoluteAddress(int pieceIndex, int offset, int pieceSize)
        {
            return pieceIndex * (long) pieceSize + offset;
        }

        public static long DivRem(long a, long b, out long result)
        {
            result = a % b;
            return a / b;
        }
    }
}