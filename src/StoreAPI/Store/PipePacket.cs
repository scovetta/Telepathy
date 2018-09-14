using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Store
{
    [Serializable]
    public class PipePacket
    {
        [Serializable]
        public enum PacketType
        {
            Ready,
            Stdout,
            SystemError,
            Eof,
        }

        string nodeName;
        PacketType type;         
        byte[] data;
        string message;
        
        public string NodeName
        {
            get { return nodeName; }
            set { nodeName = value; }
        }

        public PacketType Type
        {
            get { return type; }
            set { type = value; }
        }

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
