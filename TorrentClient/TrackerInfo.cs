using System;
using System.Collections.Generic;
using System.Net;
using Torrent.Client.Events;

namespace Torrent.Client
{
    public class TrackerInfo
    {
        private readonly TrackerClient _client;

        public TrackerInfo(string url)
        {
            Url = url;
            LastAnnounced = DateTime.MinValue;
            LastState = AnnounceState.None;
            _client = new TrackerClient(new[] {Url});
        }

        public DateTime LastAnnounced { get; private set; }
        public string Url { get; }
        public AnnounceState LastState { get; private set; }
        public TimeSpan Period { get; private set; }

        public void Started(InfoHash hash, long downloaded, long uploaded, long remaining)
        {
            Announce(hash, downloaded, uploaded, remaining, EventType.Started);
        }

        public void Stopped(InfoHash hash, long downloaded, long uploaded, long remaining)
        {
            Announce(hash, downloaded, uploaded, remaining, EventType.Stopped);
        }

        public void Completed(InfoHash hash, long downloaded, long uploaded, long remaining)
        {
            Announce(hash, downloaded, uploaded, remaining, EventType.Completed);
        }

        public void Regular(InfoHash hash, long downloaded, long uploaded, long remaining)
        {
            Announce(hash, downloaded, uploaded, remaining, EventType.None);
        }

        private void Announce(InfoHash hash, long downloaded, long uploaded, long remaining, EventType type)
        {
            var request = new TrackerRequest(hash, Global.Instance.PeerId, Global.Instance.ListeningPort,
                uploaded, downloaded, remaining, true, false, type);
            SendRequest(request);
        }

        private void SendRequest(TrackerRequest request)
        {
            var response = _client.GetResponse(request);
            LastAnnounced = DateTime.Now;
            if (response == null || response.FailureReason != null)
            {
                if (request.Event == EventType.Started)
                    LastState = AnnounceState.StartFailure;
                else
                    LastState = AnnounceState.Failure;
                Period = TimeSpan.FromSeconds(20);
            }
            else
            {
                LastState = AnnounceState.Success;
                Period = TimeSpan.FromSeconds(response.Interval);
                OnPeersReceived(response.Endpoints);
            }
        }

        public event EventHandler<EventArgs<IEnumerable<IPEndPoint>>> PeersReceived;

        public void OnPeersReceived(IEnumerable<IPEndPoint> e)
        {
            PeersReceived?.Invoke(this, new EventArgs<IEnumerable<IPEndPoint>>(e));
        }
    }
}