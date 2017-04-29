//
// Listener.cs
//
// Authors:
//   Alan McGovern alan.mcgovern@gmail.com
//
// Copyright (C) 2008 Alan McGovern
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Net.BitTorrent.Common;

namespace System.Net.BitTorrent.Client
{
    public abstract class Listener : IListener
    {
        public event EventHandler<EventArgs> StatusChanged;

        private IPEndPoint endpoint;
        private ListenerStatus status;

        public IPEndPoint Endpoint
        {
            get { return endpoint; }
        }

        public ListenerStatus Status
        {
            get { return status; }
        }


        protected Listener(IPEndPoint endpoint)
        {
            this.status = ListenerStatus.NotListening;
            this.endpoint = endpoint;
        }

        public void ChangeEndpoint(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
            if (Status == ListenerStatus.Listening)
            {
                Stop();
                Start();
            }
        }

        protected virtual void RaiseStatusChanged(ListenerStatus status)
        {
            this.status = status;
            if (StatusChanged != null)
                Toolbox.RaiseAsyncEvent<EventArgs>(StatusChanged, this, EventArgs.Empty);
        }

        public abstract void Start();

        public abstract void Stop();
    }
}
