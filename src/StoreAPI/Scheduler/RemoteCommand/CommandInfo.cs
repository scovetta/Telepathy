using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Defines property values used by the command.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get this interface, call the 
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateCommandInfo(Microsoft.Hpc.Scheduler.INameValueCollection,System.String,System.String)" /> method.</para> 
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidIRemoteComandInfo)]
    public interface ICommandInfo
    {
        /// <summary>
        ///   <para>Retrieves the startup directory for the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The full path to the startup directory.</para>
        /// </value>
        string WorkingDirectory { get; }

        /// <summary>
        ///   <para>Retrieves the environment variables used by the command.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> interface that contains the collection of environment variables used by the command.</para> 
        /// </value>
        INameValueCollection EnvironmentVariables { get; }

        /// <summary>
        ///   <para>Retrieves the path to the standard input file used by the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The full path to the standard input file used by the command.</para>
        /// </value>
        string StdIn { get; }
    }

    /// <summary>
    ///   <para>Defines property values used by the command.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.ICommandInfo" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidRemoteComandInfoClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class CommandInfo : ICommandInfo
    {
        string workDir;
        INameValueCollection envVars;
        string stdIn;

        /// <summary>
        ///   <para>Initializes a new instance of this class.</para>
        /// </summary>
        /// <param name="envVars">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" />  interface that contains the collection of environment variables used by the command.</para> 
        /// </param>
        /// <param name="workDir">
        ///   <para>The full path to the startup directory.</para>
        /// </param>
        /// <param name="stdIn">
        ///   <para>The full path to the standard input file used by the command.</para>
        /// </param>
        public CommandInfo(INameValueCollection envVars, string workDir, string stdIn)
        {
            this.workDir = workDir;
            this.envVars = envVars;
            this.stdIn = stdIn;
        }

        #region ICommandInfo Members

        /// <summary>
        ///   <para>Retrieves the startup directory for the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The full path to the startup directory.</para>
        /// </value>
        public string WorkingDirectory
        {
            get { return workDir; }
        }

        /// <summary>
        ///   <para>Retrieves the environment variables used by the command.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> interface that contains the collection of environment variables used by the command.</para> 
        /// </value>
        public INameValueCollection EnvironmentVariables
        {
            get { return envVars; }
        }

        /// <summary>
        ///   <para>Retrieves the path to the standard input file used by the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The full path to the standard input file used by the command.</para>
        /// </value>
        public string StdIn
        {
            get { return stdIn; }
        }

        #endregion
    }
}
