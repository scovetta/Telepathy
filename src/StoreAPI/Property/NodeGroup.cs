using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines a node group.</para>
    /// </summary>
    [Serializable]
    public class NodeGroup
    {
        string name;
        string description;

        /// <summary>
        ///   <para>Retrieves the name of the node group.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the node group.</para>
        /// </value>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        ///   <para>Retrieves the description of the node group.</para>
        /// </summary>
        /// <value>
        ///   <para>The description of the node group.</para>
        /// </value>
        public string Description
        {
            get { return description; }
        }

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.NodeGroup" /> class.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>The name of the node group.</para>
        /// </param>
        /// <param name="description">
        ///   <para>A description of the node group.</para>
        /// </param>
        public NodeGroup(string name, string description)
        {
            this.name = name;
            this.description = description;
        }
    }
}
