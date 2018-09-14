using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler
{
    // This is duplicating the storeAPI's event code and args (ISchedulerStore.cs)
    // Should keep it the same with storeAPI
    /// <summary>
    ///   <para>Defines constants that indicate whether an application connected to, 
    /// disconnected from, or reconnected to the HPC Job Scheduler Service or to  
    /// the channel that delivers events from the HPC Job Scheduler Service that 
    /// relate to changes to the states and properties of jobs, tasks, and nodes.</para> 
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code>const None = 0
    /// const Connect = 1
    /// const StoreDisconnect = 2
    /// const StoreReconnect = 3
    /// const Exception = 4
    /// const EventDisconnect = 5
    /// const EventReconnect = 6</code>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.IConnectionEventArg.Code" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidConnectionEventCode)]
    public enum ConnectionEventCode
    {
        /// <summary>
        ///   <para>Not used. This enumeration member represents a value of 0.</para>
        /// </summary>
        None = 0,
        /// <summary>
        ///   <para>The application connected to the HPC Job Scheduler Service or to the channel that delivers events from the HPC 
        /// Job Scheduler Service that relate to changes to the states of jobs, tasks, and nodes. This enumeration member represents a value of 1.</para>
        /// </summary>
        Connect = 1,
        /// <summary>
        ///   <para>The application disconnected from the HPC Job Scheduler Service. This enumeration member represents a value of 2.</para>
        /// </summary>
        StoreDisconnect = 2,
        /// <summary>
        ///   <para>The application reconnected to the HPC Job Scheduler Service after 
        /// getting disconnected from that service. This enumeration member represents a value of 3.</para>
        /// </summary>
        StoreReconnect = 3,
        /// <summary>
        ///   <para>An exception occurred while the application tried to connect to 
        /// the HPC Job Scheduler Service. This enumeration member represents a value of 4.</para>
        /// </summary>
        Exception = 4,
        /// <summary>
        ///   <para>The application disconnected from the channel that delivers events from the HPC Job Scheduler Service that 
        /// relate to changes to the states of jobs, tasks, and nodes. This enumeration member represents a value of 5.</para>
        /// </summary>
        EventDisconnect = 5,
        /// <summary>
        ///   <para>The application reconnected to the channel that delivers events from the HPC Job 
        /// Scheduler Service after getting disconnected from that channel. This enumeration member represents a value of 6.</para>
        /// </summary>
        EventReconnect = 6,
    }

    /// <summary>
    ///   <para>Defines the parameters that are passed to the 
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.ReconnectHandler" /> event handler when an application connects to, disconnects from, or reconnects to the HPC Job Scheduler Service or the channel that delivers events from the HPC Job Scheduler Service that relate to changes to the states of jobs, tasks, and nodes.</para> 
    /// </summary>
    /// <remarks>
    ///   <para>For information about how to implement your event handler, see the 
    /// <see cref="Microsoft.Hpc.Scheduler.ReconnectHandler" /> delegate. This event handler is called for 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OnSchedulerReconnect" /> events.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ReconnectHandler" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.OnSchedulerReconnect" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidConnectionEventArg)]
    public interface IConnectionEventArg
    {
        /// <summary>
        ///   <para>Gets a value that indicates whether the application connected to, disconnected from, or reconnected to the HPC Job Scheduler Service or 
        /// to the channel that delivers events from the HPC Job Scheduler Service that relate to changes to the states of jobs, tasks, and nodes.</para>
        /// </summary>
        /// <value>
        ///   <para>A value from the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ConnectionEventCode" /> enumeration that indicates whether the application connected to, disconnected from, or reconnected to the HPC Job Scheduler Service or to the channel that delivers events from the HPC Job Scheduler Service that relate to changes to the states of jobs, tasks, and nodes.</para> 
        /// </value>
        /// <remarks>
        ///   <para>If this value is 
        /// <see cref="Microsoft.Hpc.Scheduler.ConnectionEventCode.Exception" />, the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IConnectionEventArg.Exception" /> property contains the exception that occurred when the application tried to connect to the HPC Job Scheduler Service.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ConnectionEventCode" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.OnSchedulerReconnect" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IConnectionEventArg.Exception" />
        ConnectionEventCode Code { get; }
        /// <summary>
        ///   <para>Gets the exception that occurred when the application tried to connect to the HPC Job Scheduler Service, if the 
        /// <see cref="Microsoft.Hpc.Scheduler.IConnectionEventArg.Code" /> property value indicates that an exception occurred.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="System.Exception" /> object for the exception that occurred when the application tried to connect to the HPC Job Scheduler Service.</para> 
        /// </value>
        Exception Exception { get; }
    }

    /// <summary>
    ///   <para />
    /// </summary>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidConnectionEventArgClass)]
    public class ConnectionEventArg : EventArgs, IConnectionEventArg
    {
        Exception _e = null;
        ConnectionEventCode _code = ConnectionEventCode.None;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public Exception Exception
        {
            get { return _e; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public ConnectionEventCode Code
        {
            get { return _code; }
        }

        internal ConnectionEventArg(ConnectionEventCode code)
        {
            _code = code;
            _e = null;
        }

        internal ConnectionEventArg(ConnectionEventCode code, Exception e)
        {
            _code = code;
            _e = e;
        }
    }

    /// <summary>
    ///   <para>Defines the delegate that your application implements when you subscribe to the 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OnSchedulerReconnect" /> event.</para>
    /// </summary>
    /// <param name="sender">
    ///   <para>A 
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.Scheduler" /> object that represents the HPC Job Scheduler Service on the head node of the HPC cluster to which your application connected or reconnected, or from which your application disconnected. Cast the object to an  
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler" /> interface.</para>
    /// </param>
    /// <param name="msg">
    ///   <para>An 
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.IConnectionEventArg" /> interface that provides information that is related to change in connectivity between your application and the HPC Job Scheduler Service on the head node of the HPC cluster.</para> 
    /// </param>
    /// <remarks>
    ///   <para>To determine how the connectivity between your application and the HPC Job Scheduler Service on 
    /// the head node of the HPC cluster changed, cast the object in the msg parameter to an  
    /// <see cref="Microsoft.Hpc.Scheduler.IConnectionEventArg" /> interface and get the value of the 
    /// <see cref="Microsoft.Hpc.Scheduler.IConnectionEventArg.Code" /> property.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.OnSchedulerReconnect" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.IConnectionEventArg" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ConnectionEventCode" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.IConnectionEventArg.Code" />
    public delegate void ReconnectHandler(object sender, ConnectionEventArg msg);
}
