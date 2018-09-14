using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Hpc.Scheduler.Properties;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.IO;

namespace Microsoft.Hpc.Scheduler.Store
{
    // This class implements a customized serialization mechanism for rowset events
    // We use it instead of the standard .net serialization (which is slow)
    // The format is
    //  [RowsetId 4][ObjectIdx 4][PrevIdx 4][RowCnt 4][RowsetEvt 4][ObjId 4][Num_props 4]
    //  ([UniqueId 4][IsNull 1][Value])*
    // where [Value] can have different format and variable length

    public class EventFormatter
    {
        public Packets.RowsetChangePacket Deserialize(Stream serializationStream)
        {
            Packets.RowsetChangePacket packet = new Packets.RowsetChangePacket();

            packet.RowsetId = ReadInt(serializationStream);
            packet.ObjectIndex = ReadInt(serializationStream);
            packet.ObjectPreviousIndex = ReadInt(serializationStream);
            packet.RowCount = ReadInt(serializationStream);
            packet.RowsetEvent = (EventType)ReadInt(serializationStream);
            packet.ObjectId = ReadInt(serializationStream);

            int propCnt = ReadInt(serializationStream);
            if (propCnt == 0)
            {
                packet.Properties = new StoreProperty[] { };
            }
            else
            {
                StoreProperty[] props = new StoreProperty[propCnt];
                packet.Properties = props;
                for (int i = 0; i < propCnt; i++)
                {
                    int uniqueID = ReadInt(serializationStream);
                    PropertyId pid = PropertyLookup.PropertyIdFromPropIndex(uniqueID);
                    if (pid == null)
                    {
                        throw new SerializationException(string.Format("Unable to find property with unique ID {0}", uniqueID));
                    }

                    bool isNull = BitConverter.ToBoolean(ReadBytes(serializationStream, 1), 0);

                    if (isNull)
                    {
                        props[i] = new StoreProperty(pid, null);
                        continue;
                    }

                    switch (pid.Type)
                    {
                        case StorePropertyType.String:
                        case StorePropertyType.StringList:
                            int strLen = ReadInt(serializationStream);
                            string strVal = string.Empty;
                            if (strLen > 0)
                            {
                                strVal = ASCIIEncoding.Unicode.GetString(ReadBytes(serializationStream, strLen), 0, strLen);
                            }
                            props[i] = new StoreProperty(pid, strVal);
                            break;

                        case StorePropertyType.Int32:
                            props[i] = new StoreProperty(pid, ReadInt(serializationStream));
                            break;

                        // For all enums, we cannot save the int directly, otherwise there will 
                        // be type problem later. We have to allocate a new enum variable to hold
                        // the value.

                        case StorePropertyType.JobPriority:
                            JobPriority priVal = (JobPriority)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, priVal);
                            break;

                        case StorePropertyType.JobState:
                            JobState jstateVal = (JobState)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, jstateVal);
                            break;

                        case StorePropertyType.JobUnitType:
                            JobUnitType jutVal = (JobUnitType)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, jutVal);
                            break;

                        case StorePropertyType.JobRuntimeType:
                            JobRuntimeType jrttVal = (JobRuntimeType)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, jrttVal);
                            break;

                        case StorePropertyType.JobNodeGroupOp:
                            JobNodeGroupOp jnodeGroupOpVal = (JobNodeGroupOp)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, jnodeGroupOpVal);
                            break;

                        case StorePropertyType.CancelRequest:
                            CancelRequest cancelVal = (CancelRequest)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, cancelVal);
                            break;

                        case StorePropertyType.FailureReason:
                            FailureReason failVal = (FailureReason)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, failVal);
                            break;

                        case StorePropertyType.JobType:
                            JobType jtVal = (JobType)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, jtVal);
                            break;

                        case StorePropertyType.ResourceState:
                            ResourceState rsVal = (ResourceState)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, rsVal);
                            break;

                        case StorePropertyType.ResourceJobPhase:
                            ResourceJobPhase rjpVal = (ResourceJobPhase)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, rjpVal);
                            break;

                        case StorePropertyType.TaskState:
                            TaskState tsVal = (TaskState)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, tsVal);
                            break;

                        case StorePropertyType.JobMessageType:
                            JobMessageType jmtVal = (JobMessageType)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, jmtVal);
                            break;

                        case StorePropertyType.TaskType:
                            TaskType ttVal = (TaskType)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, ttVal);
                            break;

                        case StorePropertyType.NodeState:
                            NodeState nsVal = (NodeState)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, nsVal);
                            break;

                        case StorePropertyType.NodeAvailability:
                            NodeAvailability naVal = (NodeAvailability)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, naVal);
                            break;

                        case StorePropertyType.NodeEvent:
                            NodeEvent neVal = (NodeEvent)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, neVal);
                            break;

                        case StorePropertyType.JobEvent:
                            JobEvent jeVal = (JobEvent)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, jeVal);
                            break;

                        case StorePropertyType.PendingReason:
                            PendingReason.ReasonCode prVal = (PendingReason.ReasonCode)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, prVal);
                            break;

                        case StorePropertyType.Error:
                            PropertyError errVal = (PropertyError)ReadInt(serializationStream);
                            props[i] = new StoreProperty(pid, errVal);
                            break;

                        case StorePropertyType.Int64:
                            props[i] = new StoreProperty(pid, BitConverter.ToInt64(ReadBytes(serializationStream, 8), 0));
                            break;

                        case StorePropertyType.DateTime:
                            long dateLong = BitConverter.ToInt64(ReadBytes(serializationStream, 8), 0);
                            props[i] = new StoreProperty(pid, DateTime.FromBinary(dateLong));
                            break;

                        case StorePropertyType.UInt32:
                            props[i] = new StoreProperty(pid, BitConverter.ToUInt32(ReadBytes(serializationStream, 4), 0));
                            break;

                        case StorePropertyType.Boolean:
                            props[i] = new StoreProperty(pid, BitConverter.ToBoolean(ReadBytes(serializationStream, 1), 0));
                            break;

                        case StorePropertyType.Guid:
                            props[i] = new StoreProperty(pid, new Guid(ReadBytes(serializationStream, 16)));
                            break;
                            
                        case StorePropertyType.Binary:
                            int byteLen = ReadInt(serializationStream);
                            byte[] buffer = new byte[0];
                            if(byteLen > 0)
                            {
                                buffer = ReadBytes(serializationStream, byteLen);
                            }
                            props[i] = new StoreProperty(pid, buffer);
                            break;

                        case StorePropertyType.JobOrderby:
                            props[i] = new StoreProperty(pid, JobOrderByList.FromInt(ReadInt(serializationStream)));
                            break;

                        case StorePropertyType.TaskId:
                            TaskId tid = new TaskId(
                                ReadInt(serializationStream),
                                ReadInt(serializationStream),
                                ReadInt(serializationStream));
                            props[i] = new StoreProperty(pid, tid);
                            break;

                        case StorePropertyType.Object:
                            props[i] = new StoreProperty(pid, null);
                            break;

                        default:
                            Debug.Assert(false, string.Format("Unable to de-serialize property of type {0} name {1} id {2} i {3} propCnt {4}", pid.Type, pid.Name, pid.UniqueId, i, propCnt));
                            break;
                    }
                }
            }
            return packet;
        }

        private byte[] ReadBytes(Stream serializationStream, int cnt)
        {
            byte[] bytes = new byte[cnt];
            int nRead = 0;
            while (nRead < cnt)
            {
                // Stream.Read() may read less bytes than requested in a single read
                nRead += serializationStream.Read(bytes, nRead, cnt - nRead);
            }
            return bytes;
        }

        private int ReadInt(Stream serializationStream)
        {
            return BitConverter.ToInt32(ReadBytes(serializationStream, 4), 0);
        }

        Dictionary<long, KeyValuePair<int, byte[]>> _cachedProps = 
            new Dictionary<long, KeyValuePair<int, byte[]>>();

        long GetKey(PropertyId pid, int objectId)
        {
            long high = pid.UniqueId;
            long low = objectId;
            return high << 32 | low;
        }

        void MakePropBytes(StoreProperty prop, out byte[] buffer, out int len)
        {
            MemoryStream serializationStream = new MemoryStream();

            WriteInt(serializationStream, prop.Id.UniqueId);
            if (prop.Value == null)
            {
                WriteBool(serializationStream, true);
            }
            else
            {
                WriteBool(serializationStream, false);

                ValueSerializer valSerializer = null;
                if (ValSerializers.TryGetValue(prop.Id.Type, out valSerializer))
                {
                    valSerializer(serializationStream, prop.Value);
                }
                else
                {
                    Debug.Assert(false);
                }
            }

            len = (int)serializationStream.Length;
            buffer = serializationStream.GetBuffer();
        }

        public void Serialize(Stream serializationStream, Packets.RowsetChangePacket packet)
        {
            WriteInt(serializationStream, packet.RowsetId);
            WriteInt(serializationStream, packet.ObjectIndex);
            WriteInt(serializationStream, packet.ObjectPreviousIndex);
            WriteInt(serializationStream, packet.RowCount);
            WriteInt(serializationStream, packet.RowsetEvent);
            WriteInt(serializationStream, packet.ObjectId);

            List<StoreProperty> props = new List<StoreProperty>();

            if (null != packet.Properties)
            {
                foreach (StoreProperty prop in packet.Properties)
                {
                    if (!PropertyLookup.IsPrivateProperty(prop.Id) && ((prop.Id.Flags & PropFlags.Custom) == 0))
                    {
                        // We do not send out private or custom properties
                        props.Add(prop);
                    }
                }
            }

            if (props.Count == 0)
            {
                WriteInt(serializationStream, 0);
            }
            else
            {
                WriteInt(serializationStream, props.Count);

                foreach (StoreProperty prop in props)
                {
                    KeyValuePair<int, byte[]> pair;
                    long key = GetKey(prop.Id, packet.ObjectId);
                    if (!_cachedProps.TryGetValue(key, out pair))
                    {
                        byte[] val = null;
                        int len = 0;
                        MakePropBytes(prop, out val, out len);
                        pair = new KeyValuePair<int, byte[]>(len, val);
                        _cachedProps[key] = pair;
                    }

                    serializationStream.Write(pair.Value, 0, pair.Key);
                }
            }
        }

        private static void WriteInt(Stream serializationStream, object val)
        {
            serializationStream.Write(BitConverter.GetBytes((int)val), 0, 4);
        }

        private static void WriteBool(Stream serializationStream, object val)
        {
            serializationStream.Write(BitConverter.GetBytes((bool)val), 0, 1);
        }

        private static void WriteString(Stream serializationStream, object val)
        {
            // Strings format : [strlen 4][string <strlen>]
            // Note the strlen is the length in bytes 
            // Here we use unicode, so it is double the size of string in chars
            byte[] strval = ASCIIEncoding.Unicode.GetBytes(val as string);
            WriteInt(serializationStream, strval.Length);
            serializationStream.Write(strval, 0, strval.Length);
        }

        private static void WriteLong(Stream serializationStream, object val)
        {
            // [int64 8]
            serializationStream.Write(BitConverter.GetBytes((long)val), 0, 8);
        }

        private static void WriteDateTime(Stream serializationStream, object val)
        {
            // [Datetime 8]
            serializationStream.Write(BitConverter.GetBytes(((DateTime)val).ToBinary()), 0, 8);
        }

        private static void WriteUint(Stream serializationStream, object val)
        {
            // [Uint 4]
            serializationStream.Write(BitConverter.GetBytes((uint)val), 0, 4);
        }

        private static void WriteGuid(Stream serializationStream, object val)
        {
            // [Guid 16]
            byte[] guidval = ((Guid)val).ToByteArray();
            serializationStream.Write(guidval, 0, 16);
        }

        private static void WriteBinary(Stream serializationStream, object val)
        {
            // [binlen 4][Binary <binlen>]
            byte[] byteval = (byte[])val;
            WriteInt(serializationStream, byteval.Length);
            serializationStream.Write(byteval, 0, byteval.Length);
        }

        private static void WriteJobOrderBy(Stream serializationStream, object val)
        {
            // [Orderby 1]
            int orderbyVal = (val as JobOrderByList).ToInt();
            WriteInt(serializationStream, orderbyVal);
        }

        private static void WriteTaskId(Stream serializationStream, object val)
        {
            // [ParentJobId 4][JobtaskId 4][InstanceId 4]
            TaskId tid = (TaskId)val;
            WriteInt(serializationStream, tid.ParentJobId);
            WriteInt(serializationStream, tid.JobTaskId);
            WriteInt(serializationStream, tid.InstanceId);
        }

        private static void WriteNothing(Stream serializationStream, object val)
        {
            // No-op
        }

        private static void NotExpected(Stream serializationStream, object val)
        {
            // Not expected, debug assert false
            Debug.Assert(false);
        }

        delegate void ValueSerializer(Stream serializationStream, object val);

        static Dictionary<StorePropertyType, ValueSerializer> ValSerializers
        {
            get
            {
                if (_valSerializers == null)
                {
                    _valSerializers = new Dictionary<StorePropertyType, ValueSerializer>();

                    lock (_valSerializers)
                    {
                        _valSerializers.Add(StorePropertyType.String, WriteString);
                        _valSerializers.Add(StorePropertyType.StringList, WriteString);

                        _valSerializers.Add(StorePropertyType.Int32, WriteInt);
                        _valSerializers.Add(StorePropertyType.JobPriority, WriteInt);
                        _valSerializers.Add(StorePropertyType.JobState, WriteInt);
                        _valSerializers.Add(StorePropertyType.JobUnitType, WriteInt);
                        _valSerializers.Add(StorePropertyType.CancelRequest, WriteInt);
                        _valSerializers.Add(StorePropertyType.FailureReason, WriteInt);
                        _valSerializers.Add(StorePropertyType.JobType, WriteInt);
                        _valSerializers.Add(StorePropertyType.ResourceState, WriteInt);
                        _valSerializers.Add(StorePropertyType.ResourceJobPhase, WriteInt);
                        _valSerializers.Add(StorePropertyType.TaskState, WriteInt);
                        _valSerializers.Add(StorePropertyType.JobMessageType, WriteInt);
                        _valSerializers.Add(StorePropertyType.TaskType, WriteInt);
                        _valSerializers.Add(StorePropertyType.NodeState, WriteInt);
                        _valSerializers.Add(StorePropertyType.NodeAvailability, WriteInt);
                        _valSerializers.Add(StorePropertyType.NodeEvent, WriteInt);
                        _valSerializers.Add(StorePropertyType.JobEvent, WriteInt);
                        _valSerializers.Add(StorePropertyType.PendingReason, WriteInt);
                        _valSerializers.Add(StorePropertyType.Error, WriteInt);

                        _valSerializers.Add(StorePropertyType.Int64, WriteLong);

                        _valSerializers.Add(StorePropertyType.DateTime, WriteDateTime);

                        _valSerializers.Add(StorePropertyType.UInt32, WriteUint);

                        _valSerializers.Add(StorePropertyType.Boolean, WriteBool);

                        _valSerializers.Add(StorePropertyType.Guid, WriteGuid);

                        _valSerializers.Add(StorePropertyType.Binary, WriteBinary);

                        _valSerializers.Add(StorePropertyType.JobOrderby, WriteJobOrderBy);

                        _valSerializers.Add(StorePropertyType.Object, WriteNothing);

                        _valSerializers.Add(StorePropertyType.AllocationList, NotExpected);

                        _valSerializers.Add(StorePropertyType.TaskId, WriteTaskId);

                        _valSerializers.Add(StorePropertyType.JobNodeGroupOp, WriteInt);
                    }
                }

                return _valSerializers;
            }
        }

        static Dictionary<StorePropertyType, ValueSerializer> _valSerializers = null;
    }

}
