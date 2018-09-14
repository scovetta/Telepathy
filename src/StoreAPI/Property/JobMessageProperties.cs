using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler.Properties
{
    //[ComVisible(true)]
    /// <summary>
    ///   <para />
    /// </summary>
    [GuidAttribute(ComGuids.GuidJobMessageType)]
    public enum JobMessageType
    {
        // Unknown message.  Messages of this type should not be logged: this is primarily used for debugging.
        /// <summary>
        ///   <para />
        /// </summary>
        NA = 0,

        // The message corresponds to the Job's pending reason.  The message code will be the Pending Reason Code.
        // There should be at most one such message record per job.
        /// <summary>
        ///   <para />
        /// </summary>
        JobPendingReason = 1,

        // The message is associated with the job failure.  The message code is the error code.  ExtraText will
        // contain the job error parameters.  There should be at most one such message record per job.
        /// <summary>
        ///   <para />
        /// </summary>
        JobFailure = 2,

        // The message is associated with a job cancellation.  The message code is the error code.  ExtraText will
        // contain the job error parameters.  There should be at most one such message record per job.
        /// <summary>
        ///   <para />
        /// </summary>
        JobCancelation = 3,

        // The message is associated with a miscellaneous task failure.  The message code is the error code.
        /// <summary>
        ///   <para />
        /// </summary>
        TaskFailure = 4,

        // The message is associated with a task cancellation.  The message code is the error code.
        /// <summary>
        ///   <para />
        /// </summary>
        TaskCancelation = 5,

        // The message is associated with tasks that exit with a non-zero error code.  The message code is
        // is the error code, and the message sub-code is the task exit code.
        /// <summary>
        ///   <para />
        /// </summary>
        TaskExecutionError = 6,

        // The message is associated with tasks that fail on a specific node.  The message code is the error code,
        // and the message sub-code is the node ID.  ExtratText will contain the name of the node.
        /// <summary>
        ///   <para />
        /// </summary>
        TaskNodeError = 7,

        // The message is associated with all tasks that failed to pass validation.
        /// <summary>
        ///   <para />
        /// </summary>
        TaskValidationError = 8,

        // This type of messages allows us to associate arbitrary warnings with a job.  Each type of
        // warning is expected to have its own code (much like error codes).  Multiple warnings may
        // be associated with the same job, although each warning needs to have its own code.        
        /// <summary>
        ///   <para />
        /// </summary>
        JobWarning = 9,
    }

    /// <summary>
    ///   <para />
    /// </summary>
    [Serializable]
    public class JobMessagePropertyIds
    {
        private JobMessagePropertyIds()
        {
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId Id
        {
            get { return StorePropertyIds.Id; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId LastMessageTime
        {
            get { return StorePropertyIds.ChangeTime; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId JobId
        {
            get { return _JobId; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId MessageType
        {
            get { return _MessageType; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId MessageCode
        {
            get { return _MessageCode; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId MessageSubCode
        {
            get { return _MessageSubCode; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId MessageCount
        {
            get { return _MessageCount; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId ExtraText
        {
            get { return _ExtraText; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId JobMessageObject
        {
            get { return StorePropertyIds.JobMessageObject; }
        }

        static private PropertyId _JobId = new PropertyId(StorePropertyType.Int32, "JobId", PropertyIdConstants.JobMessagePropertyIdStart + 1);
        static private PropertyId _MessageType = new PropertyId(StorePropertyType.JobMessageType, "MessageType", PropertyIdConstants.JobMessagePropertyIdStart + 2);
        static private PropertyId _MessageCode = new PropertyId(StorePropertyType.Int32, "MessageCode", PropertyIdConstants.JobMessagePropertyIdStart + 3);
        static private PropertyId _MessageSubCode = new PropertyId(StorePropertyType.Int32, "MessageSubCode", PropertyIdConstants.JobMessagePropertyIdStart + 4);
        static private PropertyId _MessageCount = new PropertyId(StorePropertyType.Int32, "MessageCount", PropertyIdConstants.JobMessagePropertyIdStart + 5);
        static private PropertyId _ExtraText = new PropertyId(StorePropertyType.String, "ExtraText", PropertyIdConstants.JobMessagePropertyIdStart + 6);

    }
}
