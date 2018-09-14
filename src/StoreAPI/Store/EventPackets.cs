using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    using Microsoft.Hpc.Scheduler.Store;

    public class Packets
    {
        [Serializable]
        public enum EventObjectClass
        {
            Nothing         = 0,
            Job             = 1,
            Task            = 2,
            Resource        = 3,
            Node            = 4,
            Profile         = 5,
            AllJobs         = 6,
            AllTasks        = 7,
            AllResources    = 8,
            AllProfiles     = 9,
            Rowset          = 10,
            AllNodes        = 11,
        }


        [Serializable]
        public class PacketBase
        {
            public int                  ConnectionId;
            public EventObjectClass     ObjectClass;
        }

        [Serializable]
        public class Hello
        {
            // This token field is never used. But for the sake of back-comp, don't delete it now.
            public ConnectionToken token;

            public int clientId;    
        }

        [Serializable]
        public class KeepAlive
        {
            public string ClusterName;
        }

        [Serializable]
        public class Goodbye
        {
            public ConnectionToken token;
            public int clientid;
        }

        [Serializable]
        public class EventPacketBase : PacketBase
        {
            public Int32            ObjectId;
            public Int32            ObjectParentId;

            public EventType        ObjectEventType;

            public ObjectType       ObjectType;
            public StoreProperty[]  Properties;
        }

        [Serializable]
        public class RowsetChangePacket
        {
            public Int32            RowsetId;
            public EventType        RowsetEvent;
            public Int32            ObjectId;
            public StoreProperty[]  Properties;
            public int              ObjectIndex;
            public int              ObjectPreviousIndex;
            public int              RowCount;
        }

        [Serializable]
        public class EventPacket
        {
            public int              ConnectionId;
            public EventObjectClass ObjectType;
            public EventType        ObjectEventType;
            public Int32            ObjectId;
            public Int32            ObjectParentId;
            public Int32            Id1;
            public Int32            Id2;
            public StoreProperty[]  Properties;

            public override string ToString()
            {
                StringBuilder bldr = new StringBuilder(1024);

                bldr.Append("event:");
                bldr.Append(ObjectEventType);
                
                bldr.Append(" object type:");
                bldr.Append(ObjectType.ToString());
                
                bldr.Append(" id:");
                bldr.Append(ObjectId);
                
                bldr.Append(" ");
                
                if (Properties != null)
                {
                    foreach (StoreProperty item in Properties)
                    {
                        bldr.Append(item.ToString());
                        bldr.Append(", ");
                    }
                }
                
                return bldr.ToString();
            }
        }

        [Serializable]
        public class RequestPacket
        {
            public string ClientName;
            public int ConnectionId;
        }
    
    
    }
}
