using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcServer
{
    /// <summary>
    /// Provides gate keeping between administrative actions (write lock) and user actions (read lock).
    /// </summary>
    internal class AdminUserGateKeeper
    {
        private readonly ReaderWriterLock _rwl = new ReaderWriterLock();

        internal DisposableLock GetAdminLock()
        {
            _rwl.AcquireWriterLock(-1);

            return new DisposableLock(true, this);
        }

        internal void ReleaseAdminLock()
        {
            _rwl.ReleaseWriterLock();
        }

        internal DisposableLock GetUserLock()
        {
            _rwl.AcquireReaderLock(-1);

            return new DisposableLock(false, this);
        }

        internal void ReleaseUserLock()
        {
            _rwl.ReleaseReaderLock();
        }

        internal AdminUserGateKeeper()
        {
        }
    }

    /// <summary>
    /// A class that enables the "Using()" construct to ensure lock release.
    /// It is intended that this class ALWAYS be used inside "Using(...".
    /// </summary>
    internal class DisposableLock : IDisposable
    {
        private bool _isAdmin = false;
        private AdminUserGateKeeper _gatekeeper = null;
        private bool _isDisposed = false;

        private void DisposeInternal()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;  // 

                if (_isAdmin)
                {
                    _gatekeeper.ReleaseAdminLock();
                }
                else
                {
                    _gatekeeper.ReleaseUserLock();
                }
            }
        }

        public void Dispose()
        {
            DisposeInternal();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Here we clean up/release the lock in the off chance that it was leaked.
        /// </summary>
        ~DisposableLock()
        {
            Dispose();
        }

        public DisposableLock(bool isAdmin, AdminUserGateKeeper gatekeeper)
        {
            _isAdmin = isAdmin;
            _gatekeeper = gatekeeper;
        }

        private DisposableLock()
        {
        }
    }
}
