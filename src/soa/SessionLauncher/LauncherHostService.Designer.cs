//------------------------------------------------------------------------------
// <copyright file="LauncherHostService.Designer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Windows service for launcher host
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.LauncherHostService
{
    using System;
    using System.Diagnostics;

    using Microsoft.Telepathy.RuntimeTrace;

    /// <summary>
    /// Launcher Host Service
    /// </summary>
    public partial class LauncherHostService : IDisposable
    {
        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.dataServiceHost != null)
                {
                    try
                    {
                        this.dataServiceHost.Close();
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "Failed to close the data service host - {0}", e);
                    }

                    this.dataServiceHost = null;
                }

                if (this.delegationHost != null)
                {
                    try
                    {
                        this.delegationHost.Close();
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "Failed to close the delegation host - {0}", e);
                    }

                    this.delegationHost = null;
                }

                if (this.launcherHost != null)
                {
                    try
                    {
                        this.launcherHost.Close();
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "Failed to close the launcher host - {0}", e);
                    }

                    this.launcherHost = null;
                }

                if (this.schedulerDelegation is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "Failed to close the scheduler delegation - {0}", e);
                    }
                }
                this.schedulerDelegation = null;


                if (this.sessionLauncher != null)
                {
                    try
                    {
                        this.sessionLauncher.Dispose();
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "Failed to close the session launcher - {0}", e);
                    }

                    this.sessionLauncher = null;
                }
            }

        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        #endregion
    }
}
