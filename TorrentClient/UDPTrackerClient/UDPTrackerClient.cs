using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Torrent.Client.UDPTrackerClient.Misc;

namespace Torrent.Client.UDPTrackerClient
{
    public class UdpTrackerClient : BaseScraper
    {
        private byte[] _currentConnectionId;

        public UdpTrackerClient(Int32 timeout) 
            : base(timeout)
        {
            _currentConnectionId = BaseCurrentConnectionId;
        }

        public IDictionary<string, ScrapeInfo> Scrape(string url, string[] hashes, TrackerRequest urlBuilder)
        {
            Dictionary<String, ScrapeInfo> returnVal = new Dictionary<string, ScrapeInfo>();

            ValidateInput(url, hashes, ScraperType.UDP);

            _currentConnectionId = BaseCurrentConnectionId;
            Int32 trasactionId = Random.Next(0, 65535);

            var uri = new Uri(url);
            var endPoint = new IPEndPoint(Dns.GetHostAddressesAsync(uri.Host).Result.FirstOrDefault(), uri.Port);

            UdpClient udpClient = new UdpClient()
            {
                DontFragment = true,
                Client =
                {
                    SendTimeout = Timeout*1000,
                    ReceiveTimeout = Timeout*1000
                }
            };

            byte[] sendBuf = _currentConnectionId.Concat(Pack.Int32(0, Pack.Endianness.Big)).Concat(Pack.Int32(trasactionId, Pack.Endianness.Big)).ToArray();

            var result = udpClient.SendAsync(sendBuf, sendBuf.Length, endPoint).Result;

            byte[] recBuf;

            try
            {
                recBuf = udpClient.ReceiveAsync().Result.Buffer;
            }
            catch (Exception)
            {
                return null;
            }

            if (recBuf == null) throw new Exception("udpClient failed to receive");
            if (recBuf.Length < 0) throw new InvalidOperationException("udpClient received no response");
            if (recBuf.Length < 16) throw new InvalidOperationException("udpClient did not receive entire response");

            UInt32 recAction = Unpack.UInt32(recBuf, 0, Unpack.Endianness.Big);
            UInt32 recTrasactionId = Unpack.UInt32(recBuf, 4, Unpack.Endianness.Big);

            if (recAction != 0 || recTrasactionId != trasactionId)
            {
                throw new Exception("Invalid response from tracker");
            }

            _currentConnectionId = CopyBytes(recBuf, 8, 8);

            byte[] hashBytes = new byte[0];
            hashBytes = hashes.Aggregate(hashBytes, (current, hash) => current.Concat(Pack.Hex(hash)).ToArray());

            int expectedLength = 8 + (12 * hashes.Length);
            Int32 key = Random.Next(0, 65535);
            sendBuf = _currentConnectionId
                .Concat(Pack.Int32(2, Pack.Endianness.Big))
                .Concat(Pack.Int32(key, Pack.Endianness.Big))
                .Concat(hashBytes)
                .ToArray();

            result = udpClient.SendAsync(sendBuf, sendBuf.Length, endPoint).Result;

            recBuf = udpClient.ReceiveAsync().Result.Buffer;

            if (recBuf == null) throw new Exception("udpClient failed to receive");
            if (recBuf.Length < 0) throw new InvalidOperationException("udpClient received no response");
            if (recBuf.Length < expectedLength) throw new InvalidOperationException("udpClient did not receive entire response");

            recAction = Unpack.UInt32(recBuf, 0, Unpack.Endianness.Big);
            recTrasactionId = Unpack.UInt32(recBuf, 4, Unpack.Endianness.Big);

            _currentConnectionId = CopyBytes(recBuf, 8, 8);

            if (recAction != 2 || recTrasactionId != key)
            {
                throw new Exception("Invalid response from tracker");
            }

            Int32 startIndex = 8;
            foreach (String hash in hashes)
            {
                UInt32 seeders = Unpack.UInt32(recBuf, startIndex, Unpack.Endianness.Big);
                UInt32 completed = Unpack.UInt32(recBuf, startIndex + 4, Unpack.Endianness.Big);
                UInt32 leachers = Unpack.UInt32(recBuf, startIndex + 8, Unpack.Endianness.Big);

                returnVal.Add(hash, new ScrapeInfo(seeders, completed, leachers, ScraperType.UDP));

                startIndex += 12;
            }

            udpClient.Dispose();

            return returnVal;
        }

        public AnnounceInfo Announce(string url, string hash, string peerId, TrackerRequest request)
        {
            return Announce(url, hash, peerId, 0, 0, 0, 2, 0, -1, 12218, 0, request);
        }

        public AnnounceInfo Announce(String url, String hash, String peerId, Int64 bytesDownloaded, Int64 bytesLeft, Int64 bytesUploaded, 
            Int32 eventTypeFilter, Int32 ipAddress, Int32 numWant, Int32 listenPort, Int32 extensions, TrackerRequest request)
        {
            List<IPEndPoint> returnValue = new List<IPEndPoint>();

            ValidateInput(url, new[] { hash }, ScraperType.UDP);

            _currentConnectionId = BaseCurrentConnectionId;
            Int32 trasactionId = Random.Next(0, 65535);

            var uri = new Uri(url);
            var endPoint = new IPEndPoint(Dns.GetHostAddressesAsync(uri.Host).Result.FirstOrDefault(), uri.Port);

            var udpClient = new UdpClient()
                {
                    DontFragment = true,
                    Client =
                        {
                            SendTimeout = Timeout*1000,
                            ReceiveTimeout = Timeout*1000
                        }
                };

            byte[] sendBuf = _currentConnectionId.Concat(Pack.Int32(0, Pack.Endianness.Big)).Concat(Pack.Int32(trasactionId, Pack.Endianness.Big)).ToArray();

            var result = udpClient.SendAsync(sendBuf, sendBuf.Length, endPoint).Result;

            byte[] recBuf;

            try
            {
                recBuf = udpClient.ReceiveAsync().Result.Buffer;
            }
            catch (Exception)
            {
                return null;
            }

            if (recBuf == null) throw new Exception("udpClient failed to receive");
            if (recBuf.Length < 0) throw new InvalidOperationException("udpClient received no response");
            if (recBuf.Length < 16) throw new InvalidOperationException("udpClient did not receive entire response");

            UInt32 recAction = Unpack.UInt32(recBuf, 0, Unpack.Endianness.Big);
            UInt32 recTrasactionId = Unpack.UInt32(recBuf, 4, Unpack.Endianness.Big);

            if (recAction != 0 || recTrasactionId != trasactionId)
            {
                throw new Exception("Invalid response from tracker");
            }

            var str = Unpack.UInt64(recBuf, 8, Unpack.Endianness.Big);
            _currentConnectionId = CopyBytes(recBuf, 8, 8);

            byte[] hashBytes = Pack.Hex(hash).ToArray();

            Int32 key = Random.Next(0, 65535);

            sendBuf = _currentConnectionId. /*connection id*/
                Concat(Pack.Int32(1, Pack.Endianness.Big)). /*action*/
                Concat(Pack.Int32(trasactionId, Pack.Endianness.Big)). /*trasaction Id*/
                Concat(request.InfoHash.ToArray()). /*hash*/
                Concat(Encoding.ASCII.GetBytes(request.PeerId)). /*my peer id*/
                Concat(Pack.Int64(request.Downloaded, Pack.Endianness.Big)). /*bytes downloaded*/
                Concat(Pack.Int64(request.Left, Pack.Endianness.Big)). /*bytes left*/
                Concat(Pack.Int64(request.Uploaded, Pack.Endianness.Big)). /*bytes uploaded*/
                Concat(Pack.Int32(2, Pack.Endianness.Big)). /*event, 0 for none, 2 for just started*/
                Concat(Pack.Int32(ipAddress, Pack.Endianness.Big)). /*ip, 0 for this one*/
                Concat(Pack.Int32(key, Pack.Endianness.Big)). /*unique key*/
                Concat(Pack.Int32(numWant, Pack.Endianness.Big)). /*num want, -1 for as many as pos*/
                Concat(Pack.Int16((short)request.Port, Pack.Endianness.Big)). /*listen port*/
                ToArray(); /*extensions*/

            result = udpClient.SendAsync(sendBuf, sendBuf.Length, endPoint).Result;

            try
            {
                recBuf = udpClient.ReceiveAsync().Result.Buffer;
            }
            catch (Exception)
            {
                return null;
            }

            recAction = Unpack.UInt32(recBuf, 0, Unpack.Endianness.Big);
            recTrasactionId = Unpack.UInt32(recBuf, 4, Unpack.Endianness.Big);

            int waitTime = (int)Unpack.UInt32(recBuf, 8, Unpack.Endianness.Big);
            int leachers = (int)Unpack.UInt32(recBuf, 12, Unpack.Endianness.Big);
            int seeders = (int)Unpack.UInt32(recBuf, 16, Unpack.Endianness.Big);

            if (recAction != 1 || recTrasactionId != trasactionId)
            {
                throw new Exception("Invalid response from tracker");
            }

            for (Int32 i = 20; i < recBuf.Length; i += 6)
            {
                UInt32 ip = Unpack.UInt32(recBuf, i);
                UInt16 port = Unpack.UInt16(recBuf, i + 4);

                returnValue.Add(new IPEndPoint(ip, port));
            }

            udpClient.Dispose();
            var t = returnValue.Select(h => h.Address).Where(k => k.ToString() == "105.112.81.138").ToArray();
            return new AnnounceInfo(returnValue, waitTime, seeders, leachers);
        }

        //public IDictionary<String, AnnounceInfo> Announce(String url, String[] hashes, String peerId)
        //{
        //    ValidateInput(url, hashes, ScraperType.UDP);

        //    Dictionary<String, AnnounceInfo> returnVal = hashes.ToDictionary(hash => hash, hash => Announce(url, hash, peerId));

        //    return returnVal;
        //}

        private static byte[] CopyBytes(byte[] bytes, Int32 start, Int32 length)
        {
            byte[] intBytes = new byte[length];
            for (int i = 0; i < length; i++) intBytes[i] = bytes[start + i];
            return intBytes;
        }
    }
}
