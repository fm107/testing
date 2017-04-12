using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Torrent.Client.Messages
{
    /// <summary>
    ///     Provides an abstract base for the message classes, as well as constructor methods.
    /// </summary>
    public abstract class PeerMessage : IPeerMessage
    {
        private static readonly Dictionary<int, Func<PeerMessage>> Messages;

        static PeerMessage()
        {
            Messages = new Dictionary<int, Func<PeerMessage>>();
            Messages.Add(ChokeMessage.Id, () => new ChokeMessage());
            Messages.Add(UnchokeMessage.Id, () => new UnchokeMessage());
            Messages.Add(InterestedMessage.Id, () => new InterestedMessage());
            Messages.Add(NotInterestedMessage.Id, () => new NotInterestedMessage());
            Messages.Add(HaveMessage.Id, () => new HaveMessage());
            Messages.Add(BitfieldMessage.Id, () => new BitfieldMessage());
            Messages.Add(RequestMessage.Id, () => new RequestMessage());
            Messages.Add(PieceMessage.Id, () => new PieceMessage());
            Messages.Add(CancelMessage.Id, () => new CancelMessage());
            Messages.Add(PortMessage.Id, () => new PortMessage());
        }

        public static PeerMessage CreateFromBytes(byte[] buffer, int offset, int count)
        {
            if (buffer.Length < offset + count)
                throw new ArgumentException("Buffer is too small.");

            PeerMessage message;

            var length = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(buffer, offset));
            var firstByte = buffer[offset];

            if (firstByte == 0 && count == 1)
                return new KeepAliveMessage();

            if (firstByte == 19 && count == 68)
            {
                message = new HandshakeMessage();
                message.FromBytes(buffer, offset, count);
                return message;
            }


            var id = buffer[offset + 4];
            if (!Messages.ContainsKey(id))
                throw new TorrentException("Unknown message.");

            message = Messages[id]();
            message.FromBytes(buffer, offset, count);
            return message;
        }

        public bool CompareByteArray(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (var i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }

        public bool CompareBitArray(BitArray a, BitArray b)
        {
            if (a.Length != b.Length) return false;
            for (var i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }

        #region IPeerMessage Members

        public abstract int MessageLength { get; }
        public abstract void FromBytes(byte[] buffer, int offset, int count);
        public abstract int ToBytes(byte[] buffer, int offset);

        public byte[] ToBytes()
        {
            var buffer = new byte[MessageLength];
            ToBytes(buffer, 0);
            return buffer;
        }

        #endregion

        #region Read/Write utility methods

        protected static byte ReadByte(byte[] buffer, int offset)
        {
            return buffer[offset];
        }

        protected static byte ReadByte(byte[] buffer, ref int offset)
        {
            var b = buffer[offset];
            offset++;
            return b;
        }

        protected static byte[] ReadBytes(byte[] buffer, int offset, int count)
        {
            return ReadBytes(buffer, ref offset, count);
        }

        protected static byte[] ReadBytes(byte[] buffer, ref int offset, int count)
        {
            var result = new byte[count];
            Buffer.BlockCopy(buffer, offset, result, 0, count);
            offset += count;
            return result;
        }

        protected static short ReadShort(byte[] buffer, int offset)
        {
            return ReadShort(buffer, ref offset);
        }

        protected static short ReadShort(byte[] buffer, ref int offset)
        {
            var ret = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, offset));
            offset += 2;
            return ret;
        }

        protected static string ReadString(byte[] buffer, int offset, int count)
        {
            return ReadString(buffer, ref offset, count);
        }

        protected static string ReadString(byte[] buffer, ref int offset, int count)
        {
            var s = Encoding.ASCII.GetString(buffer, offset, count);
            offset += count;
            return s;
        }

        protected static int ReadInt(byte[] buffer, int offset)
        {
            return ReadInt(buffer, ref offset);
        }

        protected static int ReadInt(byte[] buffer, ref int offset)
        {
            var ret = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, offset));
            offset += 4;
            return ret;
        }

        protected static long ReadLong(byte[] buffer, int offset)
        {
            return ReadLong(buffer, ref offset);
        }

        protected static long ReadLong(byte[] buffer, ref int offset)
        {
            var ret = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer, offset));
            offset += 8;
            return ret;
        }

        protected static int Write(byte[] buffer, int offset, byte value)
        {
            buffer[offset] = value;
            return 1;
        }

        protected static int Write(byte[] dest, int destOffset, byte[] src, int srcOffset, int count)
        {
            Buffer.BlockCopy(src, srcOffset, dest, destOffset, count);
            return count;
        }

        protected static int Write(byte[] buffer, int offset, ushort value)
        {
            return Write(buffer, offset, (short) value);
        }

        protected static int Write(byte[] buffer, int offset, short value)
        {
            offset += Write(buffer, offset, (byte) (value >> 8));
            offset += Write(buffer, offset, (byte) value);
            return 2;
        }

        protected static int Write(byte[] buffer, int offset, int value)
        {
            offset += Write(buffer, offset, (byte) (value >> 24));
            offset += Write(buffer, offset, (byte) (value >> 16));
            offset += Write(buffer, offset, (byte) (value >> 8));
            offset += Write(buffer, offset, (byte) value);
            return 4;
        }

        protected static int Write(byte[] buffer, int offset, uint value)
        {
            return Write(buffer, offset, (int) value);
        }

        protected static int Write(byte[] buffer, int offset, long value)
        {
            offset += Write(buffer, offset, (int) (value >> 32));
            offset += Write(buffer, offset, (int) value);
            return 8;
        }

        protected static int Write(byte[] buffer, int offset, ulong value)
        {
            return Write(buffer, offset, (long) value);
        }

        protected static int Write(byte[] buffer, int offset, byte[] value)
        {
            return Write(buffer, offset, value, 0, value.Length);
        }

        protected static int Write(byte[] buffer, int offset, byte[] value, int length)
        {
            return Write(buffer, offset, value, 0, length);
        }

        protected static int WriteAscii(byte[] buffer, int offset, string text)
        {
            for (var i = 0; i < text.Length; i++)
                Write(buffer, offset + i, (byte) text[i]);
            return text.Length;
        }

        #endregion
    }
}