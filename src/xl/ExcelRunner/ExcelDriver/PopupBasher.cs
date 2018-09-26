//------------------------------------------------------------------------------
// <copyright file="PopupBasher.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Popup Bashing behavior
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using Accessibility;
    using Microsoft.Hpc.Excel.Internal;
    using Microsoft.Hpc.Excel.Win32;

    /// <summary>
    /// Handles popups that occur in the Excel UI and report them textually
    /// </summary>
    internal class PopupBasher : IDisposable
    {        
        /// <summary>
        /// Class name of Excel's main window. This is published and therefore should be reliable.
        /// </summary>
        private const string EXCELWINCLASSNAME = "XLMAIN";

        /// <summary>
        /// Number of intervals which a popup must remain open before warning the user
        /// </summary>
        private const int INTERVALSTOWARN = 10;

        /// <summary>
        /// List of windows that are currently open and how long they've been open.
        /// </summary>
        private static Dictionary<int, WindowStatus> historicalWindows = new Dictionary<int, WindowStatus>();

        /// <summary>
        /// Process ID of Excel
        /// </summary>
        private static int excelProcessId = 0;

        /// <summary>
        /// Timer used for periodic updates
        /// </summary>
        private System.Timers.Timer myTimer;

        /// <summary>
        /// List of bashed popups
        /// </summary>
        private List<PopupMessage> bashedPopups;

        /// <summary>
        /// Initializes a new instance of the PopupBasher class, including the timer and popup list.
        /// </summary>
        public PopupBasher()
        {
            this.myTimer = new System.Timers.Timer();
            this.myTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.CheckPopups);
            this.myTimer.Interval = PopupBasherConfiguration.Instance.Period;
            this.myTimer.Enabled = true;

            this.bashedPopups = new List<PopupMessage>();
        }

        /// <summary>
        /// Gets or sets the Excel Process ID field
        /// </summary>
        internal static int ExcelProcessId
        {
            get
            {
                return excelProcessId;
            }

            set
            {
                excelProcessId = value;
            }
        }

        /// <summary>
        /// Gets the list of bashed popups
        /// </summary>
        internal List<PopupMessage> BashedPopups
        {
            get
            {
                return this.bashedPopups;
            }
        }

        /// <summary>
        /// Disposes the current popup basher
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Helper method that logs a default action to the Application event log.
        /// </summary>
        /// <param name="parenttitle">Title of parent window</param>
        /// <param name="parentclassname">Class of parent</param>
        /// <param name="childtitle">Title of child element</param>
        /// <param name="childclassname">Class of child</param>
        /// <param name="role">Role of child</param>
        private static void LogDefaultAction(
            string parenttitle,
            string parentclassname,
            string childtitle,
            string childclassname,
            PopupBasherConfiguration.RoleSystem role)
        {
            Tracing.WriteDebugTextWarning(
                                            Tracing.ComponentId.ExcelDriver,
                                            Resources.PopupBasher_DefaultAction,
                                            parenttitle,
                                            parentclassname,
                                            childtitle,
                                            childclassname,
                                            role.ToString());
        }

        /// <summary>
        /// Callback used for EnumWindows. Checks for visible windows in the Excel process.
        /// </summary>
        /// <param name="hwnd">Handle to Window</param>
        /// <param name="lparam">User defined int value</param>
        /// <returns>Continuation status</returns>
        private static bool HandleWindow(int hwnd, int lparam)
        {
            try
            {
                IntPtr hwndPtr = (IntPtr)hwnd;
                uint processId = 0;

                // Check if window is within Excel process
                uint threadid = NativeMethods.GetWindowThreadProcessId(hwndPtr, out processId);
                if (threadid != 0 && processId == (uint)PopupBasher.ExcelProcessId)
                {
                    // If we've already seen this window, increment the count, otherwise add to list
                    if (historicalWindows.ContainsKey(hwnd))
                    {
                        WindowStatus state = historicalWindows[hwnd];
                        state.Open = true;
                        state.Recurrences = state.Recurrences + 1;
                        historicalWindows[hwnd] = state;
                    }
                    else
                    {
                        WindowStatus state;
                        state.Open = true;
                        state.Recurrences = 1;
                        historicalWindows.Add(hwnd, state);
                    }
                }
            }
            catch
            {
                // If there is an error, ignore it and look at the remaining windows.
            }

            return true;
        }

        /// <summary>
        /// Performs the resource release of the current popup basher
        /// </summary>
        /// <param name="disposing">true if called from public dispose</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.myTimer.Dispose();
            }
        }

        /// <summary>
        /// Checks popup
        /// </summary>
        /// <param name="sender"> Sender of the event</param>
        /// <param name="evt">Event data</param>
        private void CheckPopups(object sender, System.Timers.ElapsedEventArgs evt)
        {
            try
            {
                // Disable the timer while this callback is running to avoid concurrent callbacks
                this.myTimer.Enabled = false;

                // Examine popup windows. 
                this.ExamineExcelPopups();
            }
            catch (Exception ex)
            {
                Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelDriver, Resources.PopupBasher_Error, ex);
            }
            finally
            {
                this.myTimer.Enabled = true;
            }
        }

        /// <summary>
        /// Identify popups that are open. Try to bash ones that have matching configurations and trace about other windows.
        /// </summary>
        private void ExamineExcelPopups()
        {
            int hresult;
            IAccessible pacc = null;
            PopupMessage windowDescription;
            Dictionary<int, WindowStatus> openWindows = new Dictionary<int, WindowStatus>();
            Guid iidIAccessible = new Guid(NativeMethods.IID_IAccessible);
            
            try
            {
                // Look at each window and proceed only if they were successfully enumerated
                hresult = NativeMethods.EnumWindows(new NativeMethods.CallBackPtr(PopupBasher.HandleWindow), 0);
                if (hresult != 0)
                {
                    // Examine each window that was around last time or opened this time
                    if (historicalWindows != null)
                    {
                        foreach (KeyValuePair<int, WindowStatus> blockingWindow in historicalWindows)
                        {
                            // If window is still around
                            if (blockingWindow.Value.Open)
                            {
                                try
                                {
                                    // Get access to the window properties
                                    IntPtr hwndPtr = (IntPtr)blockingWindow.Key;
                                    hresult = NativeMethods.AccessibleObjectFromWindow(hwndPtr, (uint)ObjIdentifier.OBJID_WINDOW, ref iidIAccessible, out pacc);
                                    if (hresult >= 0)
                                    {
                                        // Get window class
                                        string windowClass = AccessibleHelper.GetWindowClassForUIElement(pacc);

                                        // If window is not a main or supporting Excel process window then it's a popup and should be examined
                                        if (windowClass != EXCELWINCLASSNAME)
                                        {
                                            // Get the description
                                            windowDescription = AccessibleHelper.GetPopupWindowMessage(pacc, NativeMethods.CHILDID_SELF);
                                        
                                            // if the window doesn't have message text or title, it can't be examined
                                            if (windowDescription.MessageText != null && windowDescription.TitleBar != null)
                                            {
                                                this.CompareToConfiguredWindows(hwndPtr, pacc, windowDescription, windowClass);

                                                // If this is the Nth time the window is seen in a row, produce a warning if any are detected. DCR 8305.
                                                if (blockingWindow.Value.Recurrences == INTERVALSTOWARN)
                                                {
                                                    // Trace potentially blocking window
                                                    Tracing.TraceEvent(XlTraceLevel.Warning, Tracing.ComponentId.ExcelDriver, string.Format(Resources.PopupBasher_BlockingWindow, windowDescription.TitleBar, windowDescription.MessageText, windowClass), new EventWriterCallback(delegate { Tracing.EventProvider.LogExcelDriver_BlockingPopup(windowDescription.TitleBar, windowDescription.MessageText, windowClass); }));
                                                }
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    // If there is an error, write a debug log stating that something went wrong in examining the window
                                    Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, Resources.PopupBasher_ExamineWindowFailed);
                                }

                                // Add to list of recurring windows. Reset state so that windows are evaluated freshly on the next timer trigger.
                                WindowStatus state = blockingWindow.Value;
                                state.Open = false;
                                openWindows.Add(blockingWindow.Key, state);         
                            }
                        }
                    }
                }
                else
                {
                    // Write debug log stating that we failed to enumerate the open windows
                    Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, Resources.PopupBasher_EnumWindowsFailed);
                }
            }
            catch (Exception ex)
            {
                // If there is an error, write a debug log stating that something went wrong in monitoring open windows
                Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, Resources.PopupBasher_TracePopupFailed + ex.ToString());
            }
            finally
            {
                // Release IAccessible COM object
                if (pacc != null)
                {
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pacc);
                }

                // Clear out the last history and move the open windows in the historical list
                historicalWindows.Clear();
                foreach (KeyValuePair<int, WindowStatus> window in openWindows)
                {
                    historicalWindows.Add(window.Key, window.Value);
                }

                openWindows.Clear();
            }
        }

        /// <summary>
        /// Compares current window to configured windows
        /// </summary>
        /// <param name="hwndPtr">Handle to window</param>
        /// <param name="pacc">Accessible representation of window</param>
        /// <param name="windowDescription">title and message of window</param>
        /// <param name="windowClass">window class</param>
        private void CompareToConfiguredWindows(IntPtr hwndPtr, IAccessible pacc, PopupMessage windowDescription, string windowClass)
        {
            IEnumerator children;
            PopupBasherConfiguration.PopupConfigChild currentChildWindow;
            IAccessible accControl = null;
            int childId;

            // Normal popup bashing behavior
            if (PopupBasherConfiguration.Instance.Windows != null && PopupBasherConfiguration.Instance.Windows.Windows != null)
            {
                // For each configured windows, search the couple windows name / class
                foreach (PopupBasherConfiguration.PopupConfigWindow currentWindow in PopupBasherConfiguration.Instance.Windows.Windows)
                {
                    if (windowDescription.TitleBar == currentWindow.Title && windowClass == currentWindow.ClassName)
                    {
                        // The parent window has been found, so act on children
                        Tracing.WriteDebugTextInfo(Tracing.ComponentId.ExcelDriver, Resources.PopupBasher_FoundParent, currentWindow.Title, currentWindow.ClassName);
                        NativeMethods.ShowWindow(hwndPtr, ShowWindowMode.SW_NORMAL);
                        NativeMethods.SetFocus(hwndPtr);

                        // Iterate thru children (form elements)
                        children = currentWindow.Children.GetEnumerator();
                        while (children.MoveNext())
                        {
                            currentChildWindow = (PopupBasherConfiguration.PopupConfigChild)children.Current;
                            Tracing.WriteDebugTextVerbose(
                                                        Tracing.ComponentId.ExcelDriver,
                                                        Resources.PopupBasher_SearchingForChild,
                                                        currentChildWindow.Title,
                                                        currentChildWindow.ClassName,
                                                        currentChildWindow.Role);

                            if (AccessibleHelper.FindChild(pacc, NativeMethods.CHILDID_SELF, currentChildWindow, out accControl, out childId))
                            {
                                // Add popup to list of bashed popups
                                this.bashedPopups.Add(windowDescription);
                                
                                Tracing.WriteDebugTextVerbose(
                                                            Tracing.ComponentId.ExcelDriver,
                                                            Resources.PopupBasher_FoundChild,
                                                            currentChildWindow.Title,
                                                            currentChildWindow.ClassName,
                                                            currentChildWindow.Role,
                                                            windowDescription.MessageText);

                                if (currentChildWindow.Action == PopupBasherConfiguration.ActionType.DoDefault)
                                {
                                    // Perform the default action
                                    LogDefaultAction(
                                                    currentWindow.Title,
                                                    currentWindow.ClassName,
                                                    currentChildWindow.Title,
                                                    currentChildWindow.ClassName,
                                                    currentChildWindow.Role);
                                    accControl.accDoDefaultAction(childId);
                                    break;
                                }
                            }
                        } // while (children.MoveNext())
                    } // if
                } // foreach popup window
            } // if windows != null
        }

        /// <summary>
        /// Structure that holds the status of a popup window
        /// </summary>
        private struct WindowStatus
        {
            /// <summary>
            /// Flag denoting whether the window is still open.
            /// </summary>
            public bool Open;

            /// <summary>
            /// Record of the number of times the window has been detected.
            /// </summary>
            public int Recurrences;
        }
    }
}
