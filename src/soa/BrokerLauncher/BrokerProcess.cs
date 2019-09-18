//------------------------------------------------------------------------------
// <copyright file="BrokerProcess.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Wrapped native operation to a broker process
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Wrapped native operation to a broker process
    /// </summary>
    [SecurityCritical]
    internal class BrokerProcess : CriticalFinalizerObject, IDisposable
    {
        /// <summary>
        /// Stores the ready timeout
        /// </summary>
        private static readonly TimeSpan readyTimeout = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Stores the timeout waiting for process exit event finish
        /// </summary>
        private static readonly TimeSpan processExitEventFinishedWaitTimeout = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Stores the broker shim file name
        /// </summary>
        private const string BrokerServiceFileName = "HpcBrokerWorker.exe";

        /// <summary>
        /// Stores the creation flag
        /// </summary>
        private const uint creationFlags = NativeMethods.CREATE_UNICODE_ENVIRONMENT | NativeMethods.CREATE_SUSPENDED | NativeMethods.CREATE_NO_WINDOW;

        /// <summary>
        /// Stores the startup info
        /// </summary>
        private NativeMethods.STARTUPINFO startupInfo;

        /// <summary>
        /// Stores the process info
        /// </summary>
        private NativeMethods.PROCESS_INFORMATION processInfo;

        /// <summary>
        /// Stores the ready wait handle
        /// </summary>
        private EventWaitHandle readyWaitHandle;

        /// <summary>
        /// Stores the exit wait handle
        /// </summary>
        private ManualResetEvent exitWaitHandle;

        /// <summary>
        /// Stores the wait handle that sets when process is exited and all exited event are finished
        /// </summary>
        private ManualResetEvent processExitAndEventFinishedWaitHandle = new ManualResetEvent(false);

        /// <summary>
        /// Stores the environment handle
        /// </summary>
        private GCHandle environmentHandle;

        /// <summary>
        /// Stores a value indicating whether the instance has been disposed
        /// </summary>
        private bool disposed;

        /// <summary>
        /// It is set to 1 when current object's dispose method is called.
        /// </summary>
        private int disposedFlag;

        /// <summary>
        /// Stores the lock object to protect disposing procedure
        /// </summary>
        private object lockThis = new object();

        /// <summary>
        /// Stores the unique id for the broker worker process
        /// </summary>
        private Guid uniqueId = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the BrokerProcess class
        /// </summary>
        public BrokerProcess()
            : this(BrokerServiceFileName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BrokerProcess class
        /// </summary>
        /// <param name="brokerFileName">indicating the broker file name</param>
        /// <param name="environments">indicating the environments</param>
        public BrokerProcess(string brokerFileName, NameValueConfigurationCollection environments)
        {
            this.startupInfo = new NativeMethods.STARTUPINFO();
            this.startupInfo.cb = Marshal.SizeOf(typeof(NativeMethods.STARTUPINFO));
            this.processInfo = new NativeMethods.PROCESS_INFORMATION();

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fileName = Path.Combine(path, brokerFileName);
            StringBuilder commandLine = null;
            TraceHelper.TraceEvent(TraceEventType.Information, "[BrokerProcess] Start broker process, FileName = {0}", fileName);

            IntPtr environmentPtr;
            if (environments != null)
            {
                this.environmentHandle = GCHandle.Alloc(ToByteArray(environments), GCHandleType.Pinned);
                environmentPtr = this.environmentHandle.AddrOfPinnedObject();
            }
            else
            {
                environmentPtr = IntPtr.Zero;
            }

            if (!NativeMethods.CreateProcess(fileName, commandLine, IntPtr.Zero, IntPtr.Zero, true, creationFlags, environmentPtr, path, ref this.startupInfo, out this.processInfo))
            {
                int errorCode = Marshal.GetLastWin32Error();
                TraceHelper.TraceEvent(TraceEventType.Error, "[BrokerProcess] Start broker process failed: {0}", errorCode);
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_FailedToStartBrokerServiceProcess, SR.FailedToStartBrokerServiceProcess, errorCode.ToString());
            }

            SafeWaitHandle handle = new SafeWaitHandle(this.processInfo.hProcess, false);
            if (handle.IsClosed || handle.IsInvalid)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[BrokerProcess] Start broker process failed because the process handle is invalid or closed.");
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_FailedToStartBrokerServiceProcess, SR.FailedToStartBrokerServiceProcess, "Handle is invalid or closed");
            }

            string uniqueWaitHandleName = BuildUniqueWaitHandle(this.Id, out this.readyWaitHandle);

            WaitOrTimerCallback brokerProcessReadyCallback = new ThreadHelper<object>(new WaitOrTimerCallback(this.BrokerProcessReadyCallback)).WaitOrTimerCallbackRoot;
            WaitOrTimerCallback processExitCallback = new ThreadHelper<object>(new WaitOrTimerCallback(this.ProcessExitCallback)).WaitOrTimerCallbackRoot;

            this.exitWaitHandle = new ManualResetEvent(false);
            this.exitWaitHandle.SafeWaitHandle = handle;

            // Register broker process exit callback
            ThreadPool.RegisterWaitForSingleObject(this.exitWaitHandle, processExitCallback, null, -1, true);

            // Register callback to be raised when broker process opened service host and is ready to initialize.
            ThreadPool.RegisterWaitForSingleObject(this.readyWaitHandle, brokerProcessReadyCallback, null, readyTimeout, true);
        }

        /// <summary>
        /// Finalizes an instance of the BrokerProcess class
        /// </summary>
        ~BrokerProcess()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the broker process exited event
        /// </summary>
        public event EventHandler Exited;

        /// <summary>
        /// Gets the broker process ready event
        /// </summary>
        public event EventHandler<BrokerProcessReadyEventArgs> Ready;

        /// <summary>
        /// Gets the unique id of the broker worker process
        /// </summary>
        public Guid UniqueId
        {
            get { return this.uniqueId; }
        }

        /// <summary>
        /// Gets the exit code
        /// </summary>
        public int ExitCode
        {
            get
            {
                uint exitCode;
                NativeMethods.GetExitCodeProcess(new SafeProcessHandle(this.processInfo.hProcess, false), out exitCode);
                return (int)exitCode;
            }
        }

        /// <summary>
        /// Gets the process id of this broker process
        /// </summary>
        public int Id
        {
            get { return this.processInfo.dwProcessId; }
        }

        /// <summary>
        /// Start the broker process by resuming the thread
        /// </summary>
        public void Start()
        {
            NativeMethods.ResumeThread(new SafeThreadHandle(this.processInfo.hThread));
        }

        /// <summary>
        /// Wait for the broker process ready
        /// </summary>
        public void WaitForReady()
        {
            lock (this.lockThis)
            {
                if (this.disposed)
                {
                    return;
                }

                int signal = WaitHandle.WaitAny(new WaitHandle[] { this.readyWaitHandle, this.exitWaitHandle }, readyTimeout, false);
                switch (signal)
                {
                    case WaitHandle.WaitTimeout:
                        // Timeout for ready, Kill process
                        try
                        {
                            this.KillBrokerProcess();
                        }
                        catch (Exception)
                        {
                            // Swallow the exception if failed to kill the custom broker process
                        }

                        ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_CustomBrokerReadyTimeout, SR.CustomBrokerReadyTimeout, readyTimeout.ToString());
                        break;
                    case 0:
                        // ReadyWaitHandle triggered, exit
                        break;
                    case 1:
                        // ExitWaitHandle triggered, throw exception
                        ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_CustomBrokerExitBeforeReady, SR.CustomBrokerExitBeforeReady, this.ExitCode.ToString());
                        break;
                    default:
                        Debug.Fail(String.Format("[BrokerProcess] Invalid signal from WaitHandle.WaitAny: {0}", signal));
                        break;
                }
            }
        }

        /// <summary>
        /// Wait until process is exited and all process exit callback is finished
        /// </summary>
        /// <remarks>
        /// This method won't be called concurrently
        /// </remarks>
        public void WaitForExit(TimeSpan timeoutToKillProcess)
        {
            lock (this.lockThis)
            {
                if (this.disposed)
                {
                    // Return immediately if the broker process has already exited and disposed
                    return;
                }

                if (!this.exitWaitHandle.WaitOne(timeoutToKillProcess, false))
                {
                    // Timeout , Kill process
                    try
                    {
                        this.KillBrokerProcess();
                    }
                    catch (Exception)
                    {
                        // Swallow the exception if failed to kill the custom broker process
                    }
                }

                // Still needs to wait until all event are finished
                if (!this.processExitAndEventFinishedWaitHandle.WaitOne(processExitEventFinishedWaitTimeout, false))
                {
                    TraceHelper.TraceError("0", "[BrokerProcess] Timeout waiting for process exit event finish.");
                }
            }
        }

        /// <summary>
        /// Kill broker process
        /// </summary>
        public void KillBrokerProcess()
        {
            TraceHelper.TraceEvent(TraceEventType.Information, "[BrokerProcess] Kill broker process.");
            SafeProcessHandle processHandle = new SafeProcessHandle(this.processInfo.hProcess, false);
            if (processHandle.IsClosed || processHandle.IsInvalid)
            {
                TraceHelper.TraceEvent(TraceEventType.Warning, "[BrokerProcess] Process handle is invalid or closed.");
                return;
            }

            if (!NativeMethods.TerminateProcess(processHandle, (int)BrokerShimExitCode.ForceExit))
            {
                int errorCode = Marshal.GetLastWin32Error();
                TraceHelper.TraceEvent(TraceEventType.Error, "[BrokerProcess] Kill broker process failed: {0}", errorCode);
                throw new Win32Exception(errorCode);
            }
        }

        /// <summary>
        /// Close the broker process, terminate the broker process
        /// </summary>
        public void Close()
        {
            try
            {
                this.KillBrokerProcess();
            }
            catch (Exception ex)
            {
                // Swallow exception
                TraceHelper.TraceEvent(TraceEventType.Warning, "[BrokerProcess].Close: Exception {0}", ex);
            }

            // Do not call dispose here as dispose will be called when the process exit callback triggered
        }

        /// <summary>
        /// Build a unique wait hanlde
        /// </summary>
        /// <param name="id">indicating the id</param>
        /// <param name="readyWaitHandle">output the wait handle</param>
        /// <returns>returns the name of this handle</returns>
        private static string BuildUniqueWaitHandle(int id, out EventWaitHandle readyWaitHandle)
        {
            string handleName = String.Format(Constant.InitializationWaitHandleNameFormat, id);
            bool createdNew;
            readyWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, handleName, out createdNew);
            if (!createdNew)
            {
                TraceHelper.RuntimeTrace.LogBrokerWorkerUnexpectedlyExit(id, String.Format("[BrokerProcess] Event {0} was already created by someone else.", handleName));
                if (!readyWaitHandle.Reset())
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    TraceHelper.RuntimeTrace.LogBrokerWorkerUnexpectedlyExit(id, String.Format("[BrokerProcess] Failed to reset handle {0}, Win32 Error Code = {1}.", handleName, errorCode));
                    throw new Win32Exception(errorCode);
                }
            }

            return handleName;
        }

        /// <summary>
        /// Encode environment to byte array
        /// </summary>
        /// <param name="sd">indicating the string dictionary of environments</param>
        /// <returns>returns the byte array</returns>
        public static byte[] ToByteArray(NameValueConfigurationCollection sd)
        {
            IDictionary envs = Environment.GetEnvironmentVariables();
            foreach (NameValueConfigurationElement pair in sd)
            {
                envs.Add(pair.Name, pair.Value);
            }

            string[] keys = new string[envs.Count];
            string[] values = new string[envs.Count];
            envs.Keys.CopyTo(keys, 0);
            envs.Values.CopyTo(values, 0);

            Array.Sort(keys, values, OrdinalCaseInsensitiveComparer.Default);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < envs.Count; i++)
            {

                builder.Append(keys[i]);
                builder.Append('=');
                builder.Append(values[i]);
                builder.Append('\0');
            }

            builder.Append('\0');
            return Encoding.Unicode.GetBytes(builder.ToString());
        }

        /// <summary>
        /// Callback when process exit
        /// </summary>
        /// <param name="state">indicating the state</param>
        /// <param name="timedOut">indicating whether the callback is triggered because of timeout</param>
        private void ProcessExitCallback(object state, bool timedOut)
        {
            TraceHelper.TraceEvent(TraceEventType.Information, "[BrokerProcess] Process exit callback occured, TimedOut = {0}, PID = {1}", timedOut, this.Id);

            try
            {
                if (this.Exited != null)
                {
                    this.Exited(this, EventArgs.Empty);
                }
            }
            finally
            {
                // Dispose this object and clean up all handles
                ((IDisposable)this).Dispose();
            }
        }

        /// <summary>
        /// Callback raised when broker process is ready to initialize or time out triggered
        /// </summary>
        /// <param name="state">indicating the state</param>
        /// <param name="timedOut">indicating a value whether the callback is raised because of timeout</param>
        private void BrokerProcessReadyCallback(object state, bool timedOut)
        {
            if (this.disposed)
            {
                return;
            }

            TraceHelper.TraceEvent(TraceEventType.Information, "[BrokerProcess] Broker process ready event triggered: Timeout = {0}, PID = {1}", timedOut, this.Id);

            if (this.Ready != null)
            {
                this.Ready(this, new BrokerProcessReadyEventArgs(timedOut));
            }
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void Dispose(bool disposing)
        {
            // Only dispose the object once.
            // #20745, HpcBroker.exe AppVerifier stops, incorrect object type for handle.
            if (Interlocked.CompareExchange(ref this.disposedFlag, 1, 0) != 0)
            {
                return;
            }

            if (disposing)
            {
                // Set all events as we are going to dispose all of them
                try
                {
                    // #20745, HpcBroker.exe AppVerifier stops, incorrect object type for handle.
                    // Try best effort to check the handle, but can't avoid race
                    // condition, because the broker worker process may exit after
                    // the check.
                    if (this.exitWaitHandle.SafeWaitHandle.IsClosed || this.exitWaitHandle.SafeWaitHandle.IsInvalid)
                    {
                        TraceHelper.TraceEvent(
                            TraceEventType.Warning,
                            "[BrokerProcess].Dispose: Broker process handle is already invalid or closed.");
                    }
                    else
                    {
                        this.exitWaitHandle.Set();
                    }
                }
                catch (Exception ex)
                {
                    TraceHelper.TraceEvent(TraceEventType.Warning, "[BrokerProcess].Dispose: Exception while set exitWaitHandle {0}, isDisposing = true", ex);
                }

                try
                {
                    this.processExitAndEventFinishedWaitHandle.Set();
                }
                catch (Exception ex)
                {
                    TraceHelper.TraceEvent(TraceEventType.Warning, "[BrokerProcess].Dispose: Exception while set processExitAndEventFinishedWaitHandle {0}, isDisposing = true", ex);
                }
            }

            lock (this.lockThis)
            {
                if (this.disposed)
                {
                    return;
                }

                if (disposing)
                {
                    try
                    {
                        this.readyWaitHandle.Close();
                    }
                    catch (Exception ex)
                    {
                        // Swallow all exceptions here
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[BrokerProcess].Dispose: Exception {0}", ex);
                    }

                    try
                    {
                        // #20745, HpcBroker.exe AppVerifier stops, incorrect object type for handle.
                        // Try best effort to check the handle, but can't avoid race
                        // condition, because the broker worker process may exit after
                        // the check.
                        if (this.exitWaitHandle.SafeWaitHandle.IsClosed || this.exitWaitHandle.SafeWaitHandle.IsInvalid)
                        {
                            TraceHelper.TraceEvent(
                                TraceEventType.Warning,
                                "[BrokerProcess].Dispose: Broker process handle is already invalid or closed.");
                        }
                        else
                        {
                            this.exitWaitHandle.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Swallow all exceptions here
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[BrokerProcess].Dispose: Exception {0}", ex);
                    }

                    try
                    {
                        this.processExitAndEventFinishedWaitHandle.Close();
                    }
                    catch (Exception ex)
                    {
                        // Swallow all exceptions here
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[BrokerProcess].Dispose: Exception {0}", ex);
                    }
                }

                try
                {
                    if (this.environmentHandle.IsAllocated)
                    {
                        this.environmentHandle.Free();
                    }
                }
                catch (Exception ex)
                {
                    // Swallow all exceptions here
                    TraceHelper.TraceEvent(TraceEventType.Warning, "[BrokerProcess].Dispose: Exception {0}", ex);
                }

                NativeMethods.SafeCloseValidHandle(new HandleRef(this.startupInfo, this.startupInfo.hStdError));
                NativeMethods.SafeCloseValidHandle(new HandleRef(this.startupInfo, this.startupInfo.hStdInput));
                NativeMethods.SafeCloseValidHandle(new HandleRef(this.startupInfo, this.startupInfo.hStdOutput));
                NativeMethods.SafeCloseValidHandle(new HandleRef(this.processInfo, this.processInfo.hProcess));
                NativeMethods.SafeCloseValidHandle(new HandleRef(this.processInfo, this.processInfo.hThread));

                this.disposed = true;
            }
        }

        /// <summary>
        /// Provide ordinal case insensitive string comparer
        /// </summary>
        private class OrdinalCaseInsensitiveComparer : IComparer
        {
            /// <summary>
            /// Gets the comparer
            /// </summary>
            internal static readonly OrdinalCaseInsensitiveComparer Default = new OrdinalCaseInsensitiveComparer();

            /// <summary>
            /// Compare two strings
            /// </summary>
            /// <param name="a">indicating one string</param>
            /// <param name="b">indicating another string</param>
            /// <returns>a integer indicating the result of comparation</returns>
            public int Compare(object a, object b)
            {
                string str = a as string;
                string str2 = b as string;
                if ((str != null) && (str2 != null))
                {
                    return String.CompareOrdinal(str.ToUpperInvariant(), str2.ToUpperInvariant());
                }

                return Comparer.Default.Compare(a, b);
            }
        }
    }
}
