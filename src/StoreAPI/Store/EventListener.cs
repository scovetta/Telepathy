namespace Microsoft.Hpc.Scheduler.Store
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Threading;

    using Microsoft.Hpc.Scheduler.Properties;

    internal interface IEventControl
    {
        void OnJobEvent(Int32 jobId, EventType eventType, StoreProperty[] props);
        void OnTaskEvent(Int32 jobId, Int32 taskSystemId, Int32 taskNiceId, Int32 taskInstanceId, EventType eventType, StoreProperty[] props);
        void OnResourceEvent(Int32 id, EventType eventType, StoreProperty[] props);
        void OnNodeEvent(Int32 id, EventType eventType, StoreProperty[] props);
        void OnProfileEvent(Int32 id, EventType eventType, StoreProperty[] props);
        void OnRowsetChange(int rowsetId, int rowCount, int objectIndex, int objectPreviousIndex, int objectId, EventType eventType, StoreProperty[] props);

        void RequestRegisterEvt();
        void GetEventData(int connectionId, DateTime lastReadEvent, out List<byte[]> eventData);
    }

    internal class EventListener
    {
        IEventControl _evtController;
        public string SchedulerNode { get; set; }
        int _port = 5999;

        Thread _thrd = null;
        volatile bool _fRunning = false;
        bool _overHttp = false;
        int _clientEventSleepPeriod = 1;

        public int ConnectionId
        {
            get { return _connectionId; }
        }

        public bool Registered
        {
            get { return _fRegistered; }
        }

        public static EventListener StartListening(string server, IEventControl evtController)
        {
            TraceHelper.TraceInfo("EventListener StartListening on {0}", server);
            EventListener listener = new EventListener(server, evtController);
            listener.Start();
            return listener;
        }

        public static EventListener StartListeningOverHttp(string server, IEventControl evtController, int connectionId, int clientEventSleepPeriod)
        {
            EventListener listener = new EventListener(server, evtController);
            listener._connectionId = connectionId;
            listener._overHttp = true;
            listener._clientEventSleepPeriod = clientEventSleepPeriod;
            listener.Start();
            return listener;
        }


        private EventListener(string server, IEventControl evtController)
        {
            Debug.Assert(evtController != null);

            _evtController = evtController;
            SchedulerNode = server;
        }


        private void Start()
        {
            _fRunning = true;
            _thrd = new Thread(new ThreadStart(_Listener));
            _thrd.IsBackground = true;
            _thrd.Start();
        }

        public void Stop()
        {
            _fRunning = false;

            Thread tCopy = Interlocked.Exchange(ref _thrd, null);

            if (tCopy != null)
            {
                CloseTcpClient();

                tCopy.Join(100);
            }
        }

        public void CloseTcpClient()
        {
            lock (_clientLock)
            {
                if (_client != null)
                {
                    try
                    {
                        //do a hard close on the socket of the client to avoid
                        //large numbers of sockets in time_wait 
                        //The hard close is fine in this case as the client is not
                        //going to read any data that arrives later.
                        _client.Client.Close(0);
                        _client = null;
                        _stm = null;
                    }
                    catch { }
                }
            }
        }

        static internal void ProcessEventPacket(IEventControl evtController, Packets.EventPacket packet)
        {
            TraceHelper.TraceVerbose("EventListener.ProcessEventPacket: Received event:{0} object type:{1}, ID:{2}, properties:{3}",
                packet.ObjectEventType,
                packet.ObjectType,
                packet.ObjectId,
                new Lazy<String>(
                    () =>
                    {
                        return packet.Properties == null
                            ? string.Empty
                            : String.Join<StoreProperty>(",", packet.Properties);
                    }));

            EventType eventType = packet.ObjectEventType;

            switch (packet.ObjectType)
            {
                case Packets.EventObjectClass.Job:
                    evtController.OnJobEvent(packet.ObjectId, eventType, packet.Properties);
                    break;

                case Packets.EventObjectClass.Task:
                    evtController.OnTaskEvent(packet.ObjectParentId, packet.ObjectId, packet.Id1, packet.Id2, eventType, packet.Properties);
                    break;

                case Packets.EventObjectClass.Resource:
                    if (packet.ObjectParentId == -1)
                    {
                        // If the resource is phantom, the client won't get any event
                        return;
                    }
                    evtController.OnResourceEvent(packet.ObjectId, eventType, packet.Properties);
                    break;

                case Packets.EventObjectClass.Node:
                    evtController.OnNodeEvent(packet.ObjectId, eventType, packet.Properties);
                    break;

                case Packets.EventObjectClass.Profile:
                    evtController.OnProfileEvent(packet.ObjectId, eventType, packet.Properties);
                    break;

                default:
                    break;
            }
        }


        TcpClient _client = null;
        NetworkStream _stm = null;
        BinaryFormatter _fmtr = null;

        volatile bool _fRegistered = false;
        int _connectionId = -1;

        bool UseIPv6(string servername)
        {
            if (!Socket.OSSupportsIPv6)
                return false;

            foreach (IPAddress address in Dns.GetHostAddresses(servername))
            {
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    return true;
            }

            return false;
        }

        object _clientLock = new object();

        NetworkStream ConnectAndGetStream(AddressFamily family)
        {
            lock (_clientLock)
            {
                _client = new TcpClient(family);
                _client.Connect(SchedulerNode, _port);
                return _client.GetStream();
            }
        }

        void ConnectToServer()
        {
            // Get connected to the server.

            // Note that we catch the exceptions here 
            // for debugging purposes.  They are handled
            // in the calling function as needed.

            try
            {
                _stm = null;
                if (UseIPv6(SchedulerNode))
                {
                    try
                    {
                        _stm = ConnectAndGetStream(AddressFamily.InterNetworkV6);
                    }
                    catch { }
                }

                if (_stm == null)
                {
                    _stm = ConnectAndGetStream(AddressFamily.InterNetwork);
                }

                this._fmtr = new BinaryFormatter();
                this._fmtr.UseInAppDomainSerializationBinder();

                // Send a hello message to the server

                Packets.Hello hello = new Packets.Hello();

                hello.token = null;

                _fmtr.Serialize(_stm, hello);

                hello = (Packets.Hello)_fmtr.Deserialize(_stm);

                _connectionId = hello.clientId;

                // If we make it here, we are now registered.

                _fRegistered = true;
            }
            catch (IOException e)
            {
                // What is unfortunate here is that we can't easily find out why
                // it failed from the exception itself.
                // If we want to do more processing we should PInvoke to 
                // WINSOCK to get the last error code. 

                TraceHelper.TraceError("EventListener: IOException at connect: {0}", e);
                throw;
            }
            catch (Exception e)
            {
                TraceHelper.TraceError("EventListener: Exception at connect {0}", e);
                throw;
            }
        }


        EventFormatter _evtFormatter = new EventFormatter();
        private void _Listener()
        {
            bool firstConnection = true;

            // As long as we are running, try to connect
            // to the server and then listen for events
            // as they happen.

            while (_fRunning)
            {
                _fRegistered = false;

                try
                {
                    // First get connected to the server.
                    if (!_overHttp)
                    {
                        ConnectToServer();
                    }
                    else
                    {
                        //the http event client got registered as soon as the 
                        //the storeserver registered with the scheduler
                        this._fRegistered = true;
                        this._fmtr = new BinaryFormatter();
                    }

                    if (firstConnection)
                    {
                        firstConnection = false;
                    }
                    else
                    {
                        // Only on reconnect
                        _evtController.RequestRegisterEvt();
                    }

                    // Now just keep reading items from the 
                    // server as they happen.  We will stop
                    // when an exception happens on a pending
                    // read.  When this process exits, the
                    // socket will be closed which will cause
                    // a *expected* IOException.

                    while (_fRunning)
                    {
                        object ob = null;

                        if (!_overHttp)
                        {
                            ob = _fmtr.Deserialize(_stm);
                            ProcessEventObject(ob);
                        }
                        else
                        {
                            List<byte[]> eventData = null;
                            _evtController.GetEventData(_connectionId, DateTime.UtcNow, out eventData);
                            if (eventData != null && eventData.Count > 0)
                            {
                                foreach (byte[] eventBytes in eventData)
                                {
                                    //over http you can get multiple events in one go
                                    if (eventBytes != null && eventBytes.Length > 0)
                                    {
                                        MemoryStream memStream = new MemoryStream(eventBytes, false);
                                        ob = _fmtr.Deserialize(memStream);
                                        if (ob is int)
                                        {
                                            ob = _evtFormatter.Deserialize(memStream);
                                        }

                                        ProcessEventObject(ob);
                                    }
                                }
                            }
                            Thread.Sleep(_clientEventSleepPeriod * 1000);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (_fRunning)
                    {
                        // This isn't good.  We have not been shutdown yet.
                        // Output the error so we can try and track this issue down.                        
                        TraceHelper.TraceError("EventListener: Exception {0}", e);
                        Thread.Sleep(100);
                    }

                    //do not dispose of stream, let closetcpclient deal with the socket and stream
                    if (!_overHttp)
                    {
                        CloseTcpClient();
                    }

                }
            }
        }

        private void ProcessEventObject(object ob)
        {
            if (ob is int)
            {
                Debug.Assert(!this._overHttp);
                // Tunnel to the new eventing serialization method
                Packets.RowsetChangePacket packet = _evtFormatter.Deserialize(_stm);

                TraceHelper.TraceVerbose(
                    "EventListener.ProcessEventObject: received rowset change event: Rowset ID:{0}, Rowset Event:{1}, ObjectId:{2}, ObjectIndex:{3}, ObjectPreviousIndex:{4}, RowCount:{5}, Properties:{6}",
                    packet.RowsetId,
                    packet.RowsetEvent,
                    packet.ObjectId,
                    packet.ObjectIndex,
                    packet.ObjectPreviousIndex,
                    packet.RowCount,
                    new Lazy<String>(
                        () =>
                        {
                            return packet.Properties == null
                                ? string.Empty
                                : string.Join<StoreProperty>(",", packet.Properties);
                        }));

                _evtController.OnRowsetChange(
                        packet.RowsetId,
                        packet.RowCount,
                        packet.ObjectIndex,
                        packet.ObjectPreviousIndex,
                        packet.ObjectId,
                        packet.RowsetEvent,
                        packet.Properties
                        );
            }
            else if (ob is Packets.EventPacket)
            {
                Packets.EventPacket packet = (Packets.EventPacket)ob;

                ProcessEventPacket(_evtController, packet);
            }
            else if (ob is Packets.RowsetChangePacket)
            {
                Packets.RowsetChangePacket packet = (Packets.RowsetChangePacket)ob;

                TraceHelper.TraceVerbose(
                    "EventListener.ProcessEventObject: received rowset change event: Rowset ID:{0}, Rowset Event:{1}, ObjectId:{2}, ObjectIndex:{3}, ObjectPreviousIndex:{4}, RowCount:{5}, Properties:{6}",
                    packet.RowsetId,
                    packet.RowsetEvent,
                    packet.ObjectId,
                    packet.ObjectIndex,
                    packet.ObjectPreviousIndex,
                    packet.RowCount,
                    new Lazy<String>(
                        () =>
                        {
                            return packet.Properties == null
                                ? string.Empty
                                : string.Join<StoreProperty>(",", packet.Properties);
                        }));

                _evtController.OnRowsetChange(
                        packet.RowsetId,
                        packet.RowCount,
                        packet.ObjectIndex,
                        packet.ObjectPreviousIndex,
                        packet.ObjectId,
                        packet.RowsetEvent,
                        packet.Properties
                        );
            }
            else if (ob is Packets.KeepAlive)
            {
                Debug.WriteLine(string.Format("EventAdviser: Cluster {0} says hi!", ((Packets.KeepAlive)ob).ClusterName));
            }
        }

    }
}
