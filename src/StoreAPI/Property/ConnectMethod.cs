using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    /// <para>This method used to connect to a cluster.</para>
    /// </summary>
    [Flags]
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidConnectMethod)]
    public enum ConnectMethod
    {
        /// <summary>
        /// <para>Undefined</para>
        /// </summary>
        Undefined = 0x0,

        /// <summary>
        /// <para>Windows Communication Foundation</para>
        /// </summary>
        WCF = 0x1,

        /// <summary>
        /// <para>.NET Remoting</para>
        /// </summary>
        Remoting = 0x2
    }
}
