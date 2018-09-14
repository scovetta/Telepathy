using System.Runtime.InteropServices;
using System.Xml;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{

    /// <summary>
    ///   <para />
    /// </summary>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidITaskV4SP1)]
    public interface ISchedulerTaskV4SP1
    {

        #region V2 methods. Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        void Refresh();

        /// <summary>
        ///   <para />
        /// </summary>
        void Commit();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="name">
        ///   <para />
        /// </param>
        /// <param name="value">
        ///   <para />
        /// </param>
        void SetEnvironmentVariable(string name, string value);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerTaskCounters GetCounters();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        INameValueCollection EnvironmentVariables { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        IStringCollection AllocatedNodes { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="name">
        ///   <para />
        /// </param>
        /// <param name="value">
        ///   <para />
        /// </param>
        void SetCustomProperty(string name, string value);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        INameValueCollection GetCustomProperties();

        #endregion

        #region V2 properties. Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String Name { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        Microsoft.Hpc.Scheduler.Properties.TaskState State { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        Microsoft.Hpc.Scheduler.Properties.TaskState PreviousState { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MinimumNumberOfCores { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MaximumNumberOfCores { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MinimumNumberOfNodes { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MaximumNumberOfNodes { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MinimumNumberOfSockets { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MaximumNumberOfSockets { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 Runtime { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.DateTime SubmitTime { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.DateTime CreateTime { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.DateTime EndTime { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.DateTime ChangeTime { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.DateTime StartTime { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 ParentJobId { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        Microsoft.Hpc.Scheduler.Properties.TaskId TaskId { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String CommandLine { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String WorkDirectory { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        IStringCollection RequiredNodes { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        IStringCollection DependsOn { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean IsExclusive { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean IsRerunnable { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String StdOutFilePath { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String StdInFilePath { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String StdErrFilePath { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 ExitCode { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 RequeueCount { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [System.ObsoleteAttribute("Please use the 'Type' property instead")]
        System.Boolean IsParametric { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 StartValue { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 EndValue { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 IncrementValue { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String ErrorMessage { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String Output { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean HasRuntime { get; }


        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Byte[] EncryptedUserBlob { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String UserBlob { get; set; }

        #endregion

        #region V3 methods. Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="cancelSubTasks">
        ///   <para />
        /// </param>
        void ServiceConclude(bool cancelSubTasks);

        #endregion

        #region V3 properties. Don't change.

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        Microsoft.Hpc.Scheduler.Properties.TaskType Type { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        string AllocatedCoreIds { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        bool IsServiceConcluded { get; }

        #endregion

        #region V3 SP1 Properties

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean FailJobOnFailure { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 FailJobOnFailureCount { get; set; }

        #endregion

        #region V4 properties. Don't change.

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String ValidExitCodes { get; set; }

        #endregion

        #region V4SP1 properties, Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        bool ExitIfPossible { get; }

        #endregion
    }

}
