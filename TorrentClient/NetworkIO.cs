using System;
using System.Net;
using System.Net.Sockets;

namespace Torrent.Client
{
    public delegate void NetworkCallback(bool success, int transmitted, object state);

    internal static class NetworkIo
    {
        private static readonly AsyncCallback EndReceiveCallback = EndReceive;
        private static readonly AsyncCallback EndSendCallback = EndSend;
        private static readonly AsyncCallback EndConnectCallback = EndConnect;

        private static readonly Cache<NetworkState> Cache = new Cache<NetworkState>();

        public static void Receive(Socket socket, byte[] buffer, int offset, int count, object state,
            NetworkCallback callback)
        {
            var data = Cache.Get().Init(socket, buffer, offset, count, callback, state);
            ReceiveBase(data);
        }

        public static void Send(Socket socket, byte[] buffer, int offset, int count, object state,
            NetworkCallback callback)
        {
            var data = Cache.Get().Init(socket, buffer, offset, count, callback, state);
            SendBase(data);
        }

        public static async void Connect(Socket socket, IPEndPoint endpoint, object state, NetworkCallback callback)
        {
            var data = Cache.Get().Init(socket, callback, state);
            try
            {
                await socket.ConnectAsync(endpoint);
                var result = new AsyncResult(EndConnectCallback, data);
                result.Complete();
            }
            catch
            {
                callback(false, 0, state);
                Cache.Put(data);
            }
        }

        private static async void SendBase(NetworkState data)
        {
            try
            {
                data.BytesTransferred = await data.Socket.SendAsync(new ArraySegment<byte>(data.Buffer, data.Offset, data.Count), SocketFlags.None); 
                new AsyncResult(EndSendCallback, data).Complete();
            }
            catch
            {
                data.Callback(false, 0, data.State);
                Cache.Put(data);
            }
        }

        private static async void ReceiveBase(NetworkState data)
        {
            try
            {
                data.BytesTransferred = await data.Socket.ReceiveAsync(new ArraySegment<byte>(data.Buffer, data.Offset, data.Remaining), SocketFlags.None);
                new AsyncResult(EndReceiveCallback, data).Complete();
            }
            catch
            {
                data.Callback(false, 0, data.State);
                Cache.Put(data);
            }
        }

        private static void EndReceive(IAsyncResult ar)
        {
            var data = (NetworkState) ar.AsyncState;
            try
            {
                var count = data.BytesTransferred;

                if (count == 0)
                {
                    data.Callback(false, 0, data.State);
                    Cache.Put(data);
                }
                else
                {
                    data.Offset += count;
                    data.Remaining -= count;
                    if (data.Remaining == 0)
                    {
                        data.Callback(true, data.Count, data.State);
                        Cache.Put(data);
                    }
                    else
                    {
                        ReceiveBase(data);
                    }
                }
            }
            catch
            {
                data.Callback(false, 0, data.State);
                Cache.Put(data);
            }
        }

        private static void EndSend(IAsyncResult ar)
        {
            var data = (NetworkState) ar.AsyncState;
            try
            {
                var count = data.BytesTransferred;

                if (count == 0)
                {
                    data.Callback(false, 0, data.State);
                    Cache.Put(data);
                }
                else
                {
                    data.Offset += count;
                    data.Remaining -= count;
                    if (data.Remaining == 0)
                    {
                        data.Callback(true, data.Count, data.State);
                        Cache.Put(data);
                    }
                    else
                    {
                        SendBase(data);
                    }
                }
            }
            catch
            {
                data.Callback(false, 0, data.State);
                Cache.Put(data);
            }
        }

        private static void EndConnect(IAsyncResult ar)
        {
            var data = (NetworkState) ar.AsyncState;
            try
            {
                data.Callback(true, 0, data.State);
            }
            catch (Exception e)
            {
                data.Callback(false, 0, data.State);
            }
            finally
            {
                Cache.Put(data);
            }
        }

        private class NetworkState : ICacheable
        {
            public int BytesTransferred { get; set; }
            public Socket Socket { get; private set; }
            public object State { get; private set; }
            public byte[] Buffer { get; private set; }
            public int Offset { get; set; }
            public int Remaining { get; set; }
            public int Count { get; private set; }
            public NetworkCallback Callback { get; private set; }
            
            public ICacheable Init()
            {
                return Init(null, null, null);
            }

            public NetworkState Init(Socket socket, byte[] buffer, int offset, int count, NetworkCallback callback,
                object state)
            {
                Socket = socket;
                State = state;
                Offset = offset;
                Remaining = count;
                Count = count;
                Buffer = buffer;
                Callback = callback;
                return this;
            }

            public NetworkState Init(Socket socket, NetworkCallback callback, object state)
            {
                Socket = socket;
                State = state;
                Offset = 0;
                Remaining = 0;
                Count = 0;
                Buffer = null;
                Callback = callback;
                return this;
            }
        }
    }
}