using System;
using System.Threading;

namespace Torrent.Client
{
    public class AsyncResult : IAsyncResult
    {
        #region Member Variables

        #endregion Member Variables

        #region Constructors

        public AsyncResult(AsyncCallback callback, object asyncState)
        {
            AsyncState = asyncState;
            Callback = callback;
            AsyncWaitHandle = new ManualResetEvent(false);
        }

        #endregion Constructors

        #region Properties

        protected internal ManualResetEvent AsyncWaitHandle { get; }

        internal AsyncCallback Callback { get; }

        protected internal Exception SavedException { get; set; }

        public object AsyncState { get; }

        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get { return AsyncWaitHandle; }
        }

        public bool CompletedSynchronously { get; private set; }

        public bool IsCompleted { get; set; }

        #endregion Properties

        #region Methods

        protected internal void Complete()
        {
            Complete(SavedException);
        }

        protected internal void Complete(Exception ex)
        {
            // Ensure we only complete once - Needed because in encryption there could be
            // both a pending send and pending receive so if there is an error, both will
            // attempt to complete the encryption handshake meaning this is called twice.
            if (IsCompleted)
                return;

            SavedException = ex;
            CompletedSynchronously = false;
            IsCompleted = true;
            AsyncWaitHandle.Set();

            Callback?.Invoke(this);
        }

        #endregion Methods
    }
}