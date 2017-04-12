﻿using System;
using System.Linq;
using System.Net;
using Torrent.Client.Bencoding;

namespace Torrent.Client
{
    /// <summary>
    ///     Provides utility methods for creating endpoints from data.
    /// </summary>
    public class PeerEndpoint
    {
        public static IPEndPoint FromBencoded(BencodedDictionary peer)
        {
            return new IPEndPoint(IPAddress.Parse((BencodedString) peer["ip"]),
                (ushort) (BencodedInteger) peer["port"]);
        }

        public static IPEndPoint FromBytes(byte[] peer)
        {
            var port = (ushort) IPAddress.HostToNetworkOrder((short) BitConverter.ToUInt16(peer, 4));
            return new IPEndPoint(new IPAddress(peer.Take(4).ToArray()),
                port);
        }
    }
}