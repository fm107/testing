using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Torrent.Client
{
    /// <summary>
    ///     Holds globally available information.
    /// </summary>
    public sealed class Global
    {
        private const int IdLength = 20;
        private const string IdHead = "-RT1111-";
        private const ushort ListenPort = 8912;
        private static volatile Global _instance = new Global();
        private static readonly object SyncRoot = new object();

        public readonly int BlockSize;
        private readonly Random _random;

        private Global()
        {
            ListeningPort = ListenPort;
            BlockSize = 1024 * 16;
            var seed = DateTime.Now.Millisecond + DateTime.Now.Minute + DateTime.Now.Day + IdHead.Length;
            _random = new Random(seed);
            PeerId = new string(GeneratePeerId().Select(b => (char) b).ToArray());

            BindSocket();
        }

        public string DownloadFolder { get; set; }

        public string Version
        {
            get { return IdHead.Trim('-').Trim('R').Trim('T'); }
        }

        /// <summary>
        ///     The current peer ID.
        /// </summary>
        public string PeerId { get; private set; }

        /// <summary>
        ///     The port the client listens on;
        /// </summary>
        public ushort ListeningPort { get; private set; }

        public Socket Listener { get; private set; }

        /// <summary>
        ///     Holds the single instance of the Torrent.Client.LocalInfo class.
        /// </summary>
        public static Global Instance
        {
            get
            {
                if (_instance == null)
                    lock (SyncRoot)
                    {
                        _instance = new Global();
                    }
                return _instance;
            }
        }

        public int NextRandom(int max)
        {
            return NextRandom(0, max);
        }

        public int NextRandom(int min, int max)
        {
            lock (_random)
            {
                return _random.Next(min, max);
            }
        }

        private void BindSocket()
        {
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(new IPEndPoint(IPAddress.Any, ListenPort));
            Listener.Listen(10);
        }

        private byte[] GeneratePeerId()
        {
            var id = new List<byte>(IdLength);
            lock (_random)
            {
                id.AddRange(Encoding.UTF8.GetBytes(IdHead));
                id.AddRange(Enumerable.Repeat(0, IdLength - IdHead.Length).Select(i => (byte) NextRandom('0', 'z')));
            }
            return id.ToArray();
        }
    }
}