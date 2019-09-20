// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session
{
    using System;
    using System.ServiceModel.Channels;
    using System.Threading;

    using Microsoft.Telepathy.Session.Interface;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// Utilities for sending heartbeat on the connection.
    /// </summary>
    public class HeartbeatHelper : IDisposable
    {
        /// <summary>
        /// The period of sending out heartbeat message
        /// </summary>
        private const int period = 25;

        /// <summary>
        /// Stores the dummy message request content
        /// </summary>
        private static readonly string DummyContent = new String('D', 1024);

        /// <summary>
        /// Timer for heartbeat
        /// </summary>
        private Timer heartbeatTimer;

        /// <summary>
        /// Object for sync
        /// </summary>
        private object syncObj = new object();

        /// <summary>
        /// BrokerClient connetion
        /// </summary>
        private IOutputChannel channel;

        /// <summary>
        /// BrokerController connection
        /// </summary>
        private IController controller;

        /// <summary>
        /// Response connection
        /// </summary>
        private IResponseServiceCallback responseCallback;

        /// <summary>
        /// Create an instance of the HeartbeatHelper.
        /// </summary>
        public HeartbeatHelper()
        {
            this.heartbeatTimer = new Timer(this.InternalKeepConnectionAlive, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(period));
        }

        /// <summary>
        /// Generate a heartbeat message
        /// </summary>
        /// <returns>returns the heartbeat message</returns>
        private static Message GenerateHeartbeatMessage()
        {
            return Message.CreateMessage(MessageVersion.Default, Constant.BrokerHeartbeatAction, DummyContent);
        }

        /// <summary>
        /// Send heartbeat message on the specified connections.
        /// </summary>
        private void InternalKeepConnectionAlive(object state)
        {
            try
            {
                // Send heartbeat message on the BrokerClient connection.
                if (this.channel != null)
                {
                    lock (this.syncObj)
                    {
                        if (this.channel != null)
                        {
                            this.channel.Send(GenerateHeartbeatMessage());
                        }
                    }
                }
            }
            catch { }

            try
            {
                // Call Ping method on the BrokerController connection.
                if (this.controller != null)
                {
                    this.controller.Ping();
                }
            }
            catch { }

            try
            {
                // Send heartbeat message on the Response connection.
                if (this.responseCallback != null)
                {
                    this.responseCallback.SendResponse(GenerateHeartbeatMessage());
                }
            }
            catch { }
        }

        /// <summary>
        /// It is used by the client side to keep the BrokerClient alive.
        /// </summary>
        public void KeepConnectionAlive(IOutputChannel channel)
        {
            lock (this.syncObj)
            {
                this.channel = channel;
            }
        }

        /// <summary>
        /// It is used by the client side to keep BrokerController alive.
        /// </summary>
        /// <param name="controller"></param>
        public void KeepConnectionAlive(IController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// Remove the IOutputChannel from the timer callback routine.
        /// </summary>
        public void RemoveChannel()
        {
            lock (this.syncObj)
            {
                this.channel = null;
            }
        }

        /// <summary>
        /// It is used by the service side to keep the Response connetion alive.
        /// </summary>
        /// <param name="responseCallback"></param>
        public void KeepConnectionAlive(IResponseServiceCallback responseCallback)
        {
            this.responseCallback = responseCallback;
        }

        #region IDisposable Members

        private bool disposed;

        ~HeartbeatHelper()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.heartbeatTimer != null)
                {
                    try
                    {
                        this.heartbeatTimer.Dispose();
                        this.heartbeatTimer = null;
                    }
                    catch
                    { }
                }
            }

            this.disposed = true;
        }

        #endregion
    }
}
