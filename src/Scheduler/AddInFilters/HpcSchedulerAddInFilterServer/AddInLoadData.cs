using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcServer
{
    /// <summary>
    /// Holds results of a load attempt on a given assembly.
    /// </summary>
    [DataContract]
    public sealed class AddInLoadData
    {
        [DataMember]
        public bool _activationFilterFound = false;
        [DataMember]
        public bool _submissionFilterFound = false;
        [DataMember]
        public bool _filterLifespanFound = false;

        private static DataContractSerializer _dcs = new DataContractSerializer(typeof(AddInLoadData));

        /// <summary>
        /// Here we serialize the current object into a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                _dcs.WriteObject(ms, this);

                ms.Flush();
                ms.Position = 0;

                byte[] loadData = new byte[ms.Length];

                    // read the serialized version into the byte array for caller
                ms.Read(loadData, 0, (int)ms.Length);

                return loadData;
            }
        }

        public static AddInLoadData Deserialize(byte[] loadDataBytes)
        {
            using (MemoryStream ms = new MemoryStream(loadDataBytes))
            {
                AddInLoadData retValue = _dcs.ReadObject(ms) as AddInLoadData;

                return retValue;
            }
        }
    }
}