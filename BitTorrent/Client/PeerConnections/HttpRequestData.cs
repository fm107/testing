using System;
using System.Collections.Generic;
using System.Text;
using System.Net.BitTorrent.Client.Messages.Standard;

namespace System.Net.BitTorrent.Client.Connections
{
    public partial class HttpConnection
    {
        private class HttpRequestData
        {
            public RequestMessage Request;
            public bool SentLength;
            public bool SentHeader;
            public int TotalToReceive;
            public int TotalReceived;

            public bool Complete
            {
                get { return TotalToReceive == TotalReceived; }
            }

            public HttpRequestData(RequestMessage request)
            {
                Request = request;
                PieceMessage m = new PieceMessage(request.PieceIndex, request.StartOffset, request.RequestLength);
                TotalToReceive = m.ByteLength;
            }
        }
    }
}
