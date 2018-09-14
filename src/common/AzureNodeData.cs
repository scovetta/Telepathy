namespace Microsoft.Hpc.Azure.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Serializable data structure encapsulating a node group
    /// For transmission to Azure
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    public struct AzureNodeGroup
    {
        /// <summary>
        /// Index of the node group (used for compression purposes)
        /// </summary>
        public uint GroupIndex;

        /// <summary>
        /// Name of the group
        /// </summary>
        public string GroupName;
    }

    /// <summary>
    /// Serializable data structure encapsulating a node
    /// For transmission to Azure
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    public struct AzureNode
    {
        /// <summary>
        /// Name of the node
        /// </summary>
        public string NodeName;

        /// <summary>
        /// Collection of groups that the node belongs to -- by index
        /// </summary>
        public uint[] Groups;
    }

    /// <summary>
    /// Serializable data structure encapsulating node data
    /// For transmission to Azure
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    public struct AzureNodeData
    {
        /// <summary>
        /// Groups relevant to this azure deployment
        /// </summary>
        public AzureNodeGroup[] groups;

        /// <summary>
        /// Nodes in this azure deployment
        /// </summary>
        public AzureNode[] nodes;

        /// <summary>
        /// Read all bytes from an open Stream
        /// </summary>
        private static byte[] ReadAllBytesFromStream(Stream stream)
        {
            byte[] buffer = new byte[8192];

            using (MemoryStream ms = new MemoryStream())
            {
                int bytesRead;

                for(;;)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    ms.Write(buffer, 0, bytesRead);
                } 
                
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Generates a compressed base-64 serialization of this Azure Node Data object
        /// </summary>
        /// <returns>a compressed base-64 serialization of this Azure Node Data object</returns>
        public string Serialize()
        {
            using( MemoryStream ms1 = new MemoryStream())
            using (MemoryStream ms2 = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms1, this);
                ms1.Seek(0, SeekOrigin.Begin);

                using (GZipStream gz = new GZipStream(ms2, CompressionMode.Compress,true))
                {
                    byte[] serializedBytes = ReadAllBytesFromStream(ms1);
                    gz.Write(serializedBytes, 0, serializedBytes.Length);
                    ms2.Seek(0, SeekOrigin.Begin);
                    return Convert.ToBase64String(ReadAllBytesFromStream(ms2));
                }
            }
        }

        /// <summary>
        /// Gets a comma delimited list of group names, given a node name
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public string GetNodeGroupList(string nodeName)
        {
            List<string> groupNames = new List<string>();

            if( nodeName == null )
            {
                foreach( AzureNodeGroup group in this.groups)
                   groupNames.Add(group.GroupName);
                return string.Join(",", groupNames.ToArray());
            }

            Dictionary<uint,string> lGroups = new Dictionary<uint,string>();

            if( this.groups != null)
            foreach( AzureNodeGroup group in this.groups)
            {
                lGroups[group.GroupIndex] = group.GroupName;
            }

            if( this.nodes != null )
            foreach( AzureNode node in this.nodes)
            {
                if( string.Equals(node.NodeName, nodeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach( uint groupIndex in node.Groups)
                    {
                        groupNames.Add(lGroups[groupIndex]);
                    }
                }
            }

            return string.Join(",",groupNames.ToArray());
        }

        /// <summary>
        /// Creates an AzureNodeData from a compressed BASE-64 serialized string
        /// </summary>
        /// <param name="dataString">The data string</param>
        /// <returns>The AzureNodeData object-tree</returns>
        public static AzureNodeData GetAzureNodeData(string dataString)
        {
            byte[] bytes = Convert.FromBase64String(dataString);

            using(MemoryStream ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (GZipStream unzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    bytes = ReadAllBytesFromStream(unzip);
                    using (MemoryStream ms2 = new MemoryStream())
                    {
                        ms2.Write(bytes, 0, bytes.Length);
                        ms2.Seek(0, SeekOrigin.Begin);
                        BinaryFormatter bf = new BinaryFormatter();
                        return (AzureNodeData)bf.Deserialize(ms2);
                    }
                }
            }
        }
    }
}