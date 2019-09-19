// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session
{
    /// <summary>
    ///   <para>Defines categories for the errors that a <see cref="Microsoft.Hpc.Scheduler.Session.SOAFaultCode" /> object represents.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code>const SessionConnectionError = &amp;H01000000
    /// const SessionError = &amp;H02000000
    /// const SessionFatalError = &amp;H03000000
    /// const ApplicationError = &amp;H04000000
    /// const BrokerProxyError = &amp;H05000000
    /// const DataApplicationError = &amp;H06300000
    /// const DataError = &amp;H06100000
    /// const DataFatalError = &amp;H06200000
    /// const Unknown = &amp;H0F000000</code>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SOAFaultCode" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SOAFaultCode.Category(System.Int32)" />
    public enum SOAFaultCodeCategory : int
    {
        /// <summary>
        ///   <para>Indicates that an error occurred with the connection of the client to the session. If the client 
        /// has a valid session identifier, the client can try to resolve this error by reattaching to the session a  
        /// limited number of times. If the client does not have a valid session identifier, the client can try to 
        /// resolve this error by creating a new session a limited number of times. This enumeration member represents a value of 0x01000000.</para> 
        /// </summary>
        SessionConnectionError = 0x01000000,

        /// <summary>
        ///   <para>Indicates than an error occurred with the SOA session itself. After 
        /// receiving this error, the client can try to resolve this error by creating a  
        /// new session a limited number of times. The client cannot resolve this error 
        /// by trying to reattach to the session. This enumeration member represents a value of 0x02000000.</para> 
        /// </summary>
        SessionError = 0x02000000,

        /// <summary>
        ///   <para>Indicates that a fatal session error occurred. When a client receives this error, the client should not try 
        /// to resolve the error by creating a new session or reattaching to the session until the root cause of the  
        /// error is resolved. This type of error usually occurs because of a problem with the installation of the application or 
        /// of Windows HPC Server 2008 R2, with the capacity allocated to the session, or with the application implementation. This enumeration member represents a value of 0x03000000.</para> 
        /// </summary>
        SessionFatalError = 0x03000000,

        /// <summary>
        ///   <para>Indicates that the error occurred at the application level and did not cause an error for the 
        /// SOA session. You can still use the session with the current connection. This enumeration member represents a value of 0x04000000.</para>
        /// </summary>
        ApplicationError = 0x04000000,

        /// <summary>
        ///   <para>Indicates that an unknown error has occurred on the broker proxy in an Windows 
        /// Azure deployment. Treat this error in the same way as you treat an error with a  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SOAFaultCodeCategory" /> value of 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SOAFaultCodeCategory.SessionFatalError" />. This enumeration member represents a value of 0x05000000.</para> 
        /// </summary>
        BrokerProxyError = 0x05000000,

        /// <summary>
        ///   <para>Indicates that an error occurred with a data operation. This enumeration member represents a value of 0x06100000.</para>
        /// </summary>
        DataError = 0x06100000,

        /// <summary>
        ///   <para>Indicates that a fatal error occurred for a data operation. This enumeration member represents a value of 0x06200000.</para>
        /// </summary>
        DataFatalError = 0x06200000,

        /// <summary>
        ///   <para>Indicates that an error occurred at the application level 
        /// for a data operation. This enumeration member represents a value of 0x06300000.</para>
        /// </summary>
        DataApplicationError = 0x06300000,


        /// <summary>
        ///   <para>Indicates that an unknown error occurred. Treat this error in the same way as you treat an error with a 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SOAFaultCodeCategory" /> value of 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SOAFaultCodeCategory.SessionFatalError" />. This enumeration member represents a value of 0x0F000000.</para> 
        /// </summary>
        Unknown = 0x0f000000
    }
}
