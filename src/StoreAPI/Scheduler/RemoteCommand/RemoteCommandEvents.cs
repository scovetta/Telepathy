using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Defines the arguments that are passed to your event handler when the state of the command changes.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To implement your event handler, see the <see cref="Microsoft.Hpc.Scheduler.CommandTaskStateHandler" /> delegate.</para>
    ///   <para>To get the state change information, cast this interface to an <see cref="Microsoft.Hpc.Scheduler.ITaskStateEventArg" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [Guid(ComGuids.GuidICommandTaskStateEventArg)]
    public interface ICommandTaskStateEventArg : ITaskStateEventArg
    {
        /// <summary>
        ///   <para>Retrieves the name of the node that is running the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the node.</para>
        /// </value>
        string NodeName { get; }

        /// <summary>
        ///   <para>Retrieves the exit code that the command set.</para>
        /// </summary>
        /// <value>
        ///   <para>The exit code that the command set.</para>
        /// </value>
        int ExitCode { get; }

        /// <summary>
        ///   <para>Retrieves the error message associated with the error that occurred while running the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The error message.</para>
        /// </value>
        string ErrorMessage { get; }

        /// <summary>
        ///   <para>Determines if the output is coming from a proxy task.</para>
        /// </summary>
        /// <value>
        ///   <para>Is true if the output is from a proxy; otherwise, false.</para>
        /// </value>
        /// <remarks>
        ///   <para>When you run a command, the server creates a separate command for each node on which the command will run. The server also creates a 
        /// single proxy command that it uses to capture the output and return the output 
        /// to the client. You can use this property to ignore state changes for the proxy.</para> 
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc853480(v=vs.85).aspx">Implementing the Event Handlers for Command Events in C#</see>.</para> 
        /// </example>
        bool IsProxy { get; }
    }

    /// <summary>
    ///   <para>Defines the arguments that are passed to your event handler when the state of the command changes.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To implement your event handler, see the <see cref="Microsoft.Hpc.Scheduler.CommandTaskStateHandler" /> delegate.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.CommandOutputEventArg" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.CommandRawOutputEventArg" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.JobStateEventArg" />
    [ComVisible(true)]
    [Guid(ComGuids.GuidCommandTaskStateEventArgClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class CommandTaskStateEventArg : TaskStateEventArg, ICommandTaskStateEventArg
    {
        string nodeName;
        int exitCode;
        string errorMessage;
        bool isProxy;

        internal CommandTaskStateEventArg(int jobId, TaskId taskId, TaskState newState, TaskState previousState, string nodeName, int exitCode, string errorMessage, bool isProxy)
            : base(jobId, taskId, newState, previousState)
        {
            this.nodeName = nodeName;
            this.exitCode = exitCode;
            this.errorMessage = errorMessage;
            this.isProxy = isProxy;
        }

        /// <summary>
        ///   <para>Retrieves the name of the node that is running the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the node.</para>
        /// </value>
        public string NodeName
        {
            get { return nodeName; }
        }

        /// <summary>
        ///   <para>Retrieves the exit code that the command set.</para>
        /// </summary>
        /// <value>
        ///   <para>The exit code that the command set.</para>
        /// </value>
        public int ExitCode
        {
            get { return exitCode; }
        }

        /// <summary>
        ///   <para>Retrieves the error message associated with the error that occurred while running the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The error message.</para>
        /// </value>
        public string ErrorMessage
        {
            get { return errorMessage; }
        }

        /// <summary>
        ///   <para>Indicates that the output is coming from a proxy task.</para>
        /// </summary>
        /// <value>
        ///   <para>Is true if the output is coming from a proxy task; otherwise, false.</para>
        /// </value>
        /// <remarks>
        ///   <para>When you run a command, the server creates a separate command for each node on which the command will run. The server also creates a 
        /// single proxy command that it uses to capture the output and return the output 
        /// to the client. You can use this property to ignore state changes for the proxy.</para> 
        /// </remarks>
        public bool IsProxy
        {
            get { return isProxy; }
        }
    }

    /// <summary>
    ///   <para>Defines the source of the output.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const output = 0
    /// const error = 1
    /// const eof = 2</code>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.CommandOutputEventArg.Type" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.CommandRawOutputEventArg.Type" />
    [ComVisible(true)]
    public enum CommandOutputType
    {
        /// <summary>
        ///   <para>The output was generated by the command (captured from 
        /// standard output or standard error). This enumeration member represents a value of 0.</para>
        /// </summary>
        Output,
        /// <summary>
        ///   <para>The output is an error message string that was generated when 
        /// the job that contains the command failed. This enumeration member represents a value of 1.</para>
        /// </summary>
        Error,
        /// <summary>
        ///   <para>There is no more output to receive. This enumeration member represents a value of 2.</para>
        /// </summary>
        Eof,
    }

    /// <summary>
    ///   <para>Defines the arguments that are passed to your event handler when the command generates a line of output on a node in the cluster.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To implement your event handler, see the <see cref="Microsoft.Hpc.Scheduler.CommandOutputHandler" /> delegate.</para>
    /// </remarks>
    [ComVisible(true)]
    [Guid(ComGuids.GuidICommandOutputEventArg)]
    public interface ICommandOutputEventArg
    {
        /// <summary>
        ///   <para>Retrieves the name of the node that ran the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the node.</para>
        /// </value>
        string NodeName { get; }

        /// <summary>
        ///   <para>Retrieves the line of output from the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The line of output. The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ICommandOutputEventArg.Type" /> property determines whether the output is from the command or is the message text that describes why the command failed.</para> 
        /// </value>
        /// <remarks>
        ///   <para>HPC uses the default encoding for the client to encode the output. To use a different encoding, set the 
        /// <see cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OutputEncoding" /> property before starting the command.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc853480(v=vs.85).aspx">Implementing the Event Handlers for Command Events in C#</see>.</para> 
        /// </example>
        string Message { get; }

        /// <summary>
        ///   <para>Identifies the source of the output.</para>
        /// </summary>
        /// <value>
        ///   <para>The source of the output. For possible values, see <see cref="Microsoft.Hpc.Scheduler.CommandOutputType" /> enumeration.</para>
        /// </value>
        CommandOutputType Type { get; }
    }

    /// <summary>
    ///   <para>Defines the arguments that are passed to your event handler when the command generates a line of output on a node in the cluster.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To implement your event handler, see the <see cref="Microsoft.Hpc.Scheduler.CommandOutputHandler" /> delegate.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.CommandRawOutputEventArg" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.CommandTaskStateEventArg" />
    [ComVisible(true)]
    [Guid(ComGuids.GuidCommandOutputEventArgClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class CommandOutputEventArg : EventArgs, ICommandOutputEventArg
    {
        string nodeName;
        string message;
        CommandOutputType type;

        internal CommandOutputEventArg(string nodeName, string message, CommandOutputType type)
        {
            this.nodeName = nodeName;
            this.message = message;
            this.type = type;
        }

        /// <summary>
        ///   <para>The name of the node that generated the output.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the node.</para>
        /// </value>
        public string NodeName
        {
            get { return nodeName; }
        }

        /// <summary>
        ///   <para>Retrieves the line of output from the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The line of output. The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.CommandOutputEventArg.Type" /> property determines whether the output is from the command or is the message text that describes why the command failed. </para> 
        /// </value>
        /// <remarks>
        ///   <para>HPC uses the default encoding for the client to encode the output. To use a different encoding, set the 
        /// <see cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OutputEncoding" /> property before starting the command.</para>
        /// </remarks>
        public string Message
        {
            get { return message; }
        }

        /// <summary>
        ///   <para>Identifies the source of the output.</para>
        /// </summary>
        /// <value>
        ///   <para>The source of the output. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.CommandOutputType" /> enumeration.</para>
        /// </value>
        public CommandOutputType Type
        {
            get { return type; }
        }
    }

    /// <summary>
    ///   <para>Defines the arguments that are passed to your event handler when the command 
    /// generates output on a node in the cluster. The output is passed as a byte blob.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To implement your event handler, see the <see cref="Microsoft.Hpc.Scheduler.CommandRawOutputHandler" /> delegate.</para>
    /// </remarks>
    [ComVisible(true)]
    [Guid(ComGuids.GuidICommandRawOutputEventArg)]
    public interface ICommandRawOutputEventArg
    {
        /// <summary>
        ///   <para>Retrieves the name of the node that ran the command that generated the output.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the node.</para>
        /// </value>
        string NodeName { get; }

        /// <summary>
        ///   <para>Retrieves the output from the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The output from the command. The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ICommandRawOutputEventArg.Type" /> property determines whether the output is from the command or is the message text that describes why the command failed.</para> 
        /// </value>
        /// <remarks>
        ///   <para>The output is a byte blob. The blob contains the output as it 
        /// was generated. The blob may contain one or more lines or be a partial line of output.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc853480(v=vs.85).aspx">Implementing the Event Handlers for Command Events in C#</see>.</para> 
        /// </example>
        byte[] Output { get; }

        /// <summary>
        ///   <para>Identifies the source of the output.</para>
        /// </summary>
        /// <value>
        ///   <para>Identifies the source of the output. For possible values, see <see cref="Microsoft.Hpc.Scheduler.CommandOutputType" /> enumeration.</para>
        /// </value>
        CommandOutputType Type { get; }
    }

    /// <summary>
    ///   <para>Defines the arguments that are passed to your event handler when the command 
    /// generates output on a node in the cluster. The output is passed as a byte blob.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To implement your event handler, see the <see cref="Microsoft.Hpc.Scheduler.CommandRawOutputHandler" /> delegate.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.CommandOutputEventArg" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.CommandTaskStateEventArg" />
    [ComVisible(true)]
    [Guid(ComGuids.GuidCommandRawOutputEventArgClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class CommandRawOutputEventArg : EventArgs, ICommandRawOutputEventArg
    {
        string nodeName;
        byte[] output;
        CommandOutputType type;

        internal CommandRawOutputEventArg(string nodeName, byte[] output, CommandOutputType type)
        {
            this.nodeName = nodeName;
            this.output = output;
            this.type = type;
        }

        /// <summary>
        ///   <para>The name of the node that generated the output.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the node.</para>
        /// </value>
        public string NodeName
        {
            get { return nodeName; }
        }

        /// <summary>
        ///   <para>Retrieves the output from the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The output from the command. The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.CommandRawOutputEventArg.Type" /> property determines whether the output is from the command or is the message text that describes why the command failed.</para> 
        /// </value>
        /// <remarks>
        ///   <para>The output is a byte blob. The blob contains the output as it was 
        /// generated. The blob may contain one or more lines or be a partial line of output. </para>
        /// </remarks>
        public byte[] Output
        {
            get { return output; }
        }

        /// <summary>
        ///   <para>Identifies the source of the output.</para>
        /// </summary>
        /// <value>
        ///   <para>The source of the output. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.CommandOutputType" /> enumeration.</para>
        /// </value>
        public CommandOutputType Type
        {
            get { return type; }
        }
    }

    /// <summary>
    ///   <para>Defines the interface that COM applications implement to handle events raised by the 
    /// <see cref="Microsoft.Hpc.Scheduler.IRemoteCommand" /> object.</para>
    /// </summary>
    [ComVisible(true)]
    [Guid(ComGuids.GuidIRemoteComandEvents)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IRemoteCommandEvents
    {
        /// <summary>
        ///   <para>Is called each time the command generates a line of output.</para>
        /// </summary>
        /// <param name="sender">
        ///   <para>The command object that sent the event.</para>
        /// </param>
        /// <param name="arg">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.CommandOutputEventArg" /> object that contains the event properties.</para>
        /// </param>
        void OnCommandOutput(object sender, CommandOutputEventArg arg);
        /// <summary>
        ///   <para>Is called each time the command generates output. The output is not encoded and is returned as a byte blob. </para>
        /// </summary>
        /// <param name="sender">
        ///   <para>The command object that sent the event.</para>
        /// </param>
        /// <param name="arg">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.CommandRawOutputEventArg" /> object that contains the event properties.</para>
        /// </param>
        void OnCommandRawOutput(object sender, CommandRawOutputEventArg arg);
        /// <summary>
        ///   <para>Is called each time the state of the command changes.</para>
        /// </summary>
        /// <param name="sender">
        ///   <para>The command object that sent the event.</para>
        /// </param>
        /// <param name="arg">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.CommandTaskStateEventArg" /> object that contains the event properties.</para>
        /// </param>
        void OnCommandTaskState(object sender, CommandTaskStateEventArg arg);
        /// <summary>
        ///   <para>Is called each time the state of the job that contains the command changes.</para>
        /// </summary>
        /// <param name="sender">
        ///   <para>The command object that sent the event.</para>
        /// </param>
        /// <param name="arg">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.JobStateEventArg" /> object that contains the event properties.</para>
        /// </param>
        void OnCommandJobState(object sender, JobStateEventArg arg);
    }
}
