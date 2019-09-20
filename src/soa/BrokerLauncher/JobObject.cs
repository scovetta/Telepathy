// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.Telepathy.RuntimeTrace;

    using Win32.SafeHandles;
    /// <summary>
    /// Wrapper of the job object
    /// </summary>
    internal class JobObject
    {
        /// <summary>
        /// Stores the job handle
        /// </summary>
        private IntPtr jobHandle;

        /// <summary>
        /// Initializes a new instance of the JobObject class
        /// </summary>
		[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public JobObject()
        {
            this.jobHandle = NativeMethods.CreateJobObject(IntPtr.Zero, null);
            NativeMethods.JobObjectExtendedLimitInformation info = new NativeMethods.JobObjectExtendedLimitInformation();
            info.LimitFlags = NativeMethods.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
            if (!NativeMethods.SetInformationJobObject(this.jobHandle, NativeMethods.JobObjectExtendedLimitInformationClass, ref info, Marshal.SizeOf(info)))
            {
                int error = Marshal.GetLastWin32Error();
                TraceHelper.TraceEvent(TraceEventType.Error, "[JobObject] Failed to set information to job object: ErrorCode = {0}", error);
                throw new Win32Exception(error);
            }
        }

        /// <summary>
        /// Assign a process to this job object
        /// </summary>
        /// <param name="process"></param>
        public void Assign(Process process)
        {
            if (!NativeMethods.AssignProcessToJobObject(this.jobHandle, new SafeProcessHandle(process.Handle, false)))
            {
                int error = Marshal.GetLastWin32Error();
                TraceHelper.TraceEvent(TraceEventType.Error, "[JobObject] Failed to assign broker launcher process to job object: ErrorCode = {0}", error);
                throw new Win32Exception(error);
            }
        }
    }
}
