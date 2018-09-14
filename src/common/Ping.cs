//------------------------------------------------------------------------------
// <copyright file="Priviligest.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
#define TRACE

#region Using directives
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
#endregion

namespace Microsoft.ComputeCluster
{
    internal class ICMP_PACKET
    {
        #region PrivateFields
        Byte i_type;    // type of message
        Byte i_code;    // sub code
        // The 3rd field in a packet should be checksum, 
        // but we don't have a field here because it will be calcuated when 
        // we serialize the packet
        UInt16 i_id;      // identifier
        UInt16 i_seq;     // sequence number
        Byte[] i_payload;

        private const int headerLength = 8;
        #endregion        

        #region Properities
        // Total size of the packet (header + payload)
        public int Length
        {
            get { return HeaderLength + PayloadLength; }
        }

        // Size of the packet header (excluding data)
        public int HeaderLength
        {
            get { return headerLength; }
        }

        // Size of the payload
        public int PayloadLength
        {
            get { return (i_payload == null) ? 0 : i_payload.Length; }
        }

        public byte PacketType
        {
            get { return i_type; }
        }

        public byte Code
        {
            get { return i_code; }
        }

        public UInt16 Identifier
        {
            get { return i_id; }
        }

        public UInt16 SequenceNumber
        {
            get { return i_seq; }
        }

        public byte[] PayLoad
        {
            get { return i_payload; }
        }

        public string PayLoadInString
        {
            get { return Encoding.ASCII.GetString(i_payload); }
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Contructor to compose a packet for sending
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="code"></param>
        /// <param name="id"></param>
        /// <param name="seq"></param>
        /// <param name="data"></param>
        public ICMP_PACKET(Byte kind, Byte code, UInt16 id, UInt16 seq, byte[] data)
        {
            this.i_type = kind;
            this.i_code = code;
            this.i_id = id;
            this.i_seq = seq;
            this.i_payload = data;
        }

        /// <summary>
        /// Constructor to build a packet based on a received buffer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public ICMP_PACKET(byte[] data, int offset, int count)
        {

            this.i_type = data[offset++];
            this.i_code = data[offset++];
            offset += 2;
            this.i_id = GetUshortFromNetworkOrder(data, offset);
            offset += 2;
            this.i_seq = GetUshortFromNetworkOrder(data, offset);
            offset += 2;
            this.i_payload = new byte[count - headerLength];
            Array.Copy(data, offset, this.i_payload, 0, this.i_payload.Length);
        }
        #endregion Constructors

        #region Methods
        internal static ICMP_PACKET CreateRequestPacket(UInt16 id, UInt16 seq, byte[] data)
        {
            return new ICMP_PACKET(8, 0, id, seq, data);
        }

        internal static UInt16 GetUshortFromNetworkOrder(byte[] networkBytes, int offset)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt16(new byte[] { networkBytes[offset + 1], networkBytes[offset] }, 0);
            }
            else
            {
                return BitConverter.ToUInt16(new byte[] { networkBytes[offset], networkBytes[offset + 1] }, 0);
            }
        }

        internal static byte[] GetBytesInNetworkOrder(UInt16 number)
        {
            byte[] bytes = BitConverter.GetBytes(number);

            if (BitConverter.IsLittleEndian)
            {
                return new byte[] { bytes[1], bytes[0] };
            }
            else
            {
                return bytes;
            }
        }

        internal byte[] Serialize()
        {
            return Serialize(this);
        }

        internal static byte[] Serialize(ICMP_PACKET packet)
        {
            // first, find out how many bytes to allocate for the serialized packet
            int packet_size = packet.Length;

            //allocate the byte array
            byte[] packetArray = new byte[packet_size];

            // now serialize the packet into the array.            
            int index = 0;
            packetArray[index++] = packet.i_type;
            packetArray[index++] = packet.i_code;
            // set checksum to 0 first, update it later 
            byte[] temp = BitConverter.GetBytes(0);
            // copy it into the packetArray at the required offset
            Array.Copy(temp, 0, packetArray, index, temp.Length);
            index += 2;
            // similarly, copy the rest.
            temp = GetBytesInNetworkOrder(packet.i_id); ;
            Array.Copy(temp, 0, packetArray, index, temp.Length);
            index += 2;
            // copy seq#
            temp = GetBytesInNetworkOrder(packet.i_seq);
            Array.Copy(temp, 0, packetArray, index, temp.Length);
            index += 2;
            // copy payload
            if (packet.PayloadLength > 0)
            {
                Array.Copy(packet.i_payload, 0, packetArray, index, packet.PayloadLength);
            }
            // Calculate checksum            
            int checksum = 0;
            for (int i = 0; i < packet_size; i += 2)
            {
                checksum += BitConverter.ToUInt16(packetArray, i);
            }

            checksum = (checksum >> 16) + (checksum & 0xffff);
            checksum += (checksum >> 16);
            temp = BitConverter.GetBytes((ushort)~checksum);
            // copy checksum to the correct place
            Array.Copy(temp, 0, packetArray, 2, temp.Length);

            return packetArray;
        }
        #endregion
    }

    /// <summary>
    /// A utility class to ping a computer using ICMP
    /// </summary>
    internal static class Ping
    {
        #region Statics
        // The payload to send to the target computer
        const string PING_PAYLOAD = "MSComputeCluster";
        static readonly byte[] PingPayLoad = Encoding.ASCII.GetBytes(PING_PAYLOAD);
        // The packet to send to the target computer
        static readonly ICMP_PACKET PingPacket = ICMP_PACKET.CreateRequestPacket(0x100, 0x100, PingPayLoad);
        static readonly byte[] PingData = PingPacket.Serialize();

        const int RECEIVE_BUFFER_SIZE = 1024;
        const int IP_PACKET_HEADER_SIZE = 20;        
        #endregion

        #region Methods
        /// <summary>
        /// Ping a host to check if it's alive
        /// </summary>
        /// <param name="ipTarget">IP address for the host to ping</param>
        /// <param name="timeout">timeout for one ping</param>
        /// <param name="retry">retry times</param>
        /// <returns></returns>
        /// <exception>SocketException will be thrown in case of network failure or timeout</exception>
        public static bool PingHost(IPAddress ipTarget, int timeout, int retry)
        {
            using (Socket pingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp))
            {
                pingSocket.SendTimeout = timeout;
                pingSocket.ReceiveTimeout = timeout;

                IPEndPoint epTarget = new IPEndPoint(ipTarget, 0);
                EndPoint epResponse = (EndPoint)new IPEndPoint(0, 0);
                byte[] receiveData = new byte[RECEIVE_BUFFER_SIZE];

                for (int i = 0; i < retry; i++)
                {
                    //send a ping packet to remote host                               
                    pingSocket.SendTo(PingData, epTarget);

                    //wait for response from the target machine, will throw SocketException if timedout                                
                    int read = pingSocket.ReceiveFrom(receiveData, ref epResponse);

                    //Succeed if the response comes from the target, no need to check the payload 
                    //since we only care about machine liveness
                    if (read > 0 && ((IPEndPoint)epResponse).Address.Equals(ipTarget))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion
    }
}