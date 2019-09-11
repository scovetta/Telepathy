//------------------------------------------------------------------------------
// <copyright file="SafeThreadHandle.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Wrapped handle for job object
// </summary>
//------------------------------------------------------------------------------
using System;
using System.Security;
using System.Security.Permissions;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public sealed class SafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeThreadHandle() : base(true) { }

        public SafeThreadHandle(IntPtr handle)
            : base(false)
        {
            this.SetHandle(handle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return NativeMethods.CloseHandle(this.handle);
        }
    }
}

