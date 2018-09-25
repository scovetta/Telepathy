//------------------------------------------------------------------------------
// <copyright file="LauncherHostService.Designer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Windows service for launcher host
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.LauncherHostService
{
    /// <summary>
    /// Launcher Host Service
    /// </summary>
    internal partial class LauncherHostService
    {
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        [method: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Target = "LauncherHostService.launcherInstance", Justification = "It's closed in OnStop().")]
        [method: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Target = "LauncherHostService.launcherHost", Justification = "It's closed in OnStop().")]
        [method: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Target = "LauncherHostService.nodeMappingCacheHost", Justification = "It's closed in OnStop().")]
        [method: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Target = "LauncherHostService.azureStorageCleaner", Justification = "It's closed in OnStop().")]
        [method: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Target = "LauncherHostService.singleProcessLock", Justification = "It's closed in OnStop().")]
        [method: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Target = "LauncherHostService.diagServiceHost", Justification = "It's closed in CloseSoaDiagService() called in OnStop().")]
        [method: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Target = "LauncherHostService.soaDiagAuthenticator", Justification = "It's closed in CloseSoaDiagService() called in OnStop().")]
        [method: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Target = "LauncherHostService.cleanupService", Justification = "It's closed in CloseSoaDiagService() called in OnStop().")]
        protected override void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                this.OnStop();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // LauncherHostService
            // 
            this.CanPauseAndContinue = true;
            this.ServiceName = "HpcBroker";

        }

        #endregion
    }
}
