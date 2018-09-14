using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Session
{
    /// <summary>
    ///   <para>Defines the transport binding schemes.</para>
    /// </summary>
    /// <remarks>
    ///   <para>This enumeration is a mask to allow for multiple front ends, each with a different transport scheme, when 
    /// creating a session. For example, by using <c>TransportScheme.NetTcp | TransportScheme.Http</c> your session could be accessed by a Windows client using  
    /// <see cref="System.ServiceModel.NetTcpBinding" />, and by a Linux client using Java to access your session with HTTP.</para>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members, and then set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const NetTcp = 1
    /// const Http = 2
    /// const WebAPI = 8</code>
    /// </remarks>
    /// <example />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.TransportScheme" />
    [Flags]
    public enum TransportScheme
    {
        /// <summary>
        ///   <para>Not used.</para>
        /// </summary>
        None = 0x0,

        /// <summary>
        ///   <para>Specifies a string constant for the NetTcp binding scheme. For details, see 
        /// <see cref="System.ServiceModel.NetTcpBinding" />. This enumeration member represents a value of 1.</para>
        /// </summary>
        NetTcp = 0x1,

        /// <summary>
        ///   <para>Specifies a string constant for the HTTP or HTTPS binding scheme, depending on the value of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Secure" /> property. For details, see 
        /// <see cref="System.ServiceModel.BasicHttpBinding" />. This enumeration member represents a value of 2.</para>
        /// </summary>
        Http = 0x2,

        /// <summary>
        /// Specifies a string constant for the custom binding scheme
        /// </summary>
        Custom = 0x4,

        /// <summary>
        ///   <para>Specifies that the session should contact the HPC Job Scheduler Service 
        /// and the broker through the HPC Web Service API based on the representational  
        /// state transfer (REST) model. This value is used only for sessions that connect 
        /// to the Windows Azure HPC Scheduler. This enumeration member represents a value of 8. </para> 
        ///   <para>This value is supported starting with Windows HPC Server 2008 R2 with Service Pack 3 (SP3).</para>
        /// </summary>
        WebAPI = 0x8,

        /// <summary>
        /// Specifies a string constant for the NetHttp binding scheme
        /// </summary>
        NetHttp = 0x10
    }
}
