﻿namespace Torrent.Client
{
    /// <summary>
    ///     Provides a container class for the BitTorrent tracker request data.
    /// </summary>
    public class TrackerRequest
    {
        /// <summary>
        ///     Initializes an instance of the Torrent.Client.TrackerRequest class with the specified parameters.
        /// </summary>
        /// <param name="infoHash">The SHA1 hash of the value of the info key from the torrent metadata.</param>
        /// <param name="peerId">The Peer ID, used to uniquely identify the peer. 20 bytes.</param>
        /// <param name="port">The port at which the client is listening on. Typically 6881-6889.</param>
        /// <param name="uploaded">The total count of uploaded bytes.</param>
        /// <param name="downloaded">The total count of downloaded bytes.</param>
        /// <param name="left">The count of bytes left to download for the download to be complete.</param>
        /// <param name="compact">Whether to request a compact response by the tracker. (binary peers representation)</param>
        /// <param name="omitPeerIds">Whether to request that the tracker should omit peer IDs in the peers dictionary.</param>
        /// <param name="event">Specifies the event that caused the request. None by default.</param>
        public TrackerRequest(InfoHash infoHash, string peerId, ushort port, long uploaded, long downloaded,
            long left, bool compact, bool omitPeerIds, EventType @event = EventType.None,
            int? numWant = null)
        {
            InfoHash = infoHash;
            PeerId = peerId;
            Port = port;
            Uploaded = uploaded;
            Downloaded = downloaded;
            Left = left;
            Compact = compact;
            OmitPeerIds = omitPeerIds;
            Event = @event;
            NumWant = numWant;
        }

        /// <summary>
        ///     The SHA1 hash of the value of the info key from the torrent metadata.
        /// </summary>
        public InfoHash InfoHash { get; private set; }

        /// <summary>
        ///     The Peer ID, used to uniquely identify the peer. 20 bytes.
        /// </summary>
        public string PeerId { get; private set; }

        /// <summary>
        ///     The port at which the client is listening on. Typically 6881-6889.
        /// </summary>
        public ushort Port { get; private set; }

        /// <summary>
        ///     The total count of uploaded bytes.
        /// </summary>
        public long Uploaded { get; private set; }

        /// <summary>
        ///     The total count of downloaded bytes.
        /// </summary>
        public long Downloaded { get; private set; }

        /// <summary>
        ///     The count of bytes left to download for the download to be complete.
        /// </summary>
        public long Left { get; private set; }

        /// <summary>
        ///     Whether to request a compact response by the tracker. (binary peers representation)
        /// </summary>
        public bool Compact { get; private set; }

        /// <summary>
        ///     Whether to request that the tracker should omit peer IDs in the peers dictionary.
        /// </summary>
        public bool OmitPeerIds { get; private set; }

        /// <summary>
        ///     The number of peers the tracker should return. Optional.
        /// </summary>
        public int? NumWant { get; private set; }

        /// <summary>
        ///     Specifies the event that caused the request.
        /// </summary>
        public EventType Event { get; private set; }
    }
}