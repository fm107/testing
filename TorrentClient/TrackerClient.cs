using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.Torrent;
using System.Text;
using System.Threading.Tasks;
using Torrent.Client.Extensions;

namespace Torrent.Client
{
    /// <summary>
    ///     Performs communication with a remote BitTorrent tracker.
    /// </summary>
    public class TrackerClient
    {
        /// <summary>
        ///     The class constructor.
        /// </summary>
        /// <param name="announces">The announce URLs of the tracker.</param>
        public TrackerClient(IEnumerable<string> announces)
        {
            Announces = announces;
        }

        /// <summary>
        ///     The announce URL of the tracker.
        /// </summary>
        public IEnumerable<string> Announces { get; }

        public string PreferredAnnounce { get; private set; }

        private static string UrlEncode(byte[] source)
        {
            var builder = new StringBuilder();
            var hex = BitConverter.ToString(source).Replace("-", string.Empty);
            hex.Batch(2).ForEach(h => builder.Append("%" + new string(h.ToArray()).ToLower()));
            return builder.ToString();
        }

        public TrackerResponse AnnounceStart(byte[] infoHash, string peerId, ushort port, long downloaded, long uploaded,
            long left)
        {
            var request = new TrackerRequest(infoHash, peerId, port, uploaded, downloaded, left, true, true,
                EventType.Started, 100);
            return GetResponse(request);
        }

        /// <summary>
        ///     Sends a HTTP request to the tracker and returns the response.
        /// </summary>
        /// <param name="requestData">The data for the request that will be sent to the tracker.</param>
        /// <returns>The tracker's response.</returns>
        public TrackerResponse GetResponse(TrackerRequest requestData)
        {
            TrackerResponse response = null;

            if (PreferredAnnounce != null)
                response = AttemptGet(requestData, PreferredAnnounce);

            if (response == null)
                foreach (var url in Announces)
                    if ((response = AttemptGet(requestData, url)) != null)
                    {
                        PreferredAnnounce = url;
                        break;
                    }

            return response;
        }

        private TrackerResponse AttemptGet(TrackerRequest requestData, string announceUrl)
        {
            var parameters = new Dictionary<string, string>
            {
                {"info_hash", UrlEncode(requestData.InfoHash)},
                {"peer_id", Uri.EscapeDataString(requestData.PeerId)},
                {"port", requestData.Port.ToString()},
                {"uploaded", requestData.Uploaded.ToString()},
                {"downloaded", requestData.Downloaded.ToString()},
                {"left", requestData.Left.ToString()},
                {"compact", requestData.Compact ? "1" : "0"},
                {"no_peer_id", requestData.OmitPeerIds ? "1" : "0"}
            };
            if (requestData.Event != EventType.None)
                parameters.Add("event", requestData.Event.ToString().ToLower());
            if (requestData.NumWant.HasValue)
                parameters.Add("numwant", requestData.NumWant.ToString());
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(parameters.Select(kv =>
            {
                if (kv.Value == null)
                    return string.Empty;
                return kv.Key + "=" + kv.Value;
            }).ToDelimitedString("&"));

            if (!announceUrl.Contains("?")) announceUrl += "?";
            else announceUrl += "&";

            if (announceUrl.Contains("udp://"))
            {
                return GetUdpResponse(announceUrl, requestData);
            }
            
            var request = (HttpWebRequest) WebRequest.Create(announceUrl + urlBuilder);
            request.Method = "GET";

            try
            {
                var response = request.GetResponseAsync().Result;

                byte[] trackerResponse;
                using (var reader = new BinaryReader(response.GetResponseStream()))
                {
                    using (var ms = new MemoryStream())
                    {
                        var buffer = new byte[1024];
                        var len = 0;
                        while ((len = reader.Read(buffer, 0, buffer.Length)) != 0)
                            ms.Write(buffer, 0, len);
                        trackerResponse = ms.ToArray();
                    }
                }
                response.Dispose();
                return new TrackerResponse(trackerResponse);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static TrackerResponse GetUdpResponse(string announceUrl, TrackerRequest urlBuilder)
        {
            var cl = new UDPTrackerClient(5000);
            BaseScraper.AnnounceInfo resp = cl.Announce(announceUrl, urlBuilder.InfoHash.ToString(), urlBuilder.PeerId, urlBuilder);

            IDictionary<string, BaseScraper.ScrapeInfo> scrap = cl.Scrape(announceUrl, new[] {urlBuilder.InfoHash.ToString()}, urlBuilder);

            var scr = scrap.Values.FirstOrDefault();
            return new TrackerResponse(scr, resp);
        }
    }
}