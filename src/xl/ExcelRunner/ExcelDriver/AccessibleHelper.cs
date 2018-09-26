//------------------------------------------------------------------------------
// <copyright file="AccessibleHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Excel UI interaction helper class
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using Accessibility;
    using Microsoft.Hpc.Excel.Internal;
    using Microsoft.Hpc.Excel.Win32;

    /// <summary>
    /// Helper class that works with popup UI to extract useful data
    /// </summary>
    internal static class AccessibleHelper
    {
        /// <summary>
        /// Returns the name of the UI element
        /// </summary>
        /// <param name="pacc"> Accessible Parent </param>
        /// <param name="varChild">Child Variant</param>
        /// <returns>Name of UI Element</returns>
        public static string GetUIElementName(IAccessible pacc, int varChild)
        {
            int numRetry = 3;
            bool retried = false;
            string name = string.Empty;
            if (null == pacc)
            {
                return name;
            }

            do
            {
                try
                {
                    name = pacc.get_accName(varChild);
                }
                catch (COMException comEx)
                {
                    // 0x80010001 OLE Server is busy, do 3 retry
                    if (comEx.ErrorCode == -2147418111)
                    {
                        retried = true;
                        numRetry--;
                    }
                }
            }
            while (numRetry > 0 && retried);

            return name;
        }

        /// <summary>
        /// Retreive the role from the UI element
        /// </summary>
        /// <param name="pacc">object representing the UI element</param>
        /// <param name="roleID">role of window containing the UI element</param>
        /// <param name="roleRef">reference to accessibility role result</param>
        /// <returns>the element role as a string</returns>
        public static string GetUIElementRole(IAccessible pacc, int roleID, ref uint roleRef)
        {
            if (null == pacc)
            {
                return string.Empty;
            }

            StringBuilder roleBuffer = new StringBuilder(256);
            object o = pacc.get_accRole(roleID);
            roleRef = Convert.ToUInt32(o, CultureInfo.InvariantCulture);

            if (roleRef >= 0)
            {
                NativeMethods.GetRoleText(roleRef, roleBuffer, (uint)roleBuffer.Capacity);
            }

            return roleBuffer.ToString();
        }

        /// <summary>
        /// This gets the Window Class of an object.
        /// </summary>
        /// <param name="pacc">Parent window as IAccessible</param>
        /// <returns>Window Class</returns>
        public static string GetWindowClassForUIElement(IAccessible pacc)
        {
            if (null == pacc)
            {
                return string.Empty;
            }

            IntPtr hwnd = IntPtr.Zero;
            StringBuilder classNameBuffer = new StringBuilder(256);

            if (NativeMethods.WindowFromAccessibleObject(pacc, ref hwnd) == 0)
            {
                if (hwnd.ToInt32() > 0)
                {
                    NativeMethods.GetClassName(hwnd, classNameBuffer, classNameBuffer.Capacity);
                }
            }

            return classNameBuffer.ToString();
        }

        /// <summary>
        /// Retreives message from popup
        /// </summary>
        /// <param name="paccParent">Parent window as IAccessible</param>
        /// <param name="childId">Id of child</param>
        /// <returns>The popup title and message</returns>
        public static PopupMessage GetPopupWindowMessage(IAccessible paccParent, int childId)
        {
            PopupMessage windowDescription = new PopupMessage();
            windowDescription.TitleBar = GetUIElementName(paccParent, childId);
            GetPopupWindowMessageHelper(paccParent, ref windowDescription);
            return windowDescription;
        }

        /// <summary>
        /// Utility function to get the popup message
        /// </summary>
        /// <param name="paccParent">Parent window as IAccessible</param>
        /// <param name="windowDescription">reference to popup message record</param>
        public static void GetPopupWindowMessageHelper(IAccessible paccParent, ref PopupMessage windowDescription)
        {
            int numChildren = paccParent.accChildCount;
            object[] children = new object[numChildren];
            NativeMethods.AccessibleChildren(paccParent, 0, numChildren, children, out numChildren);
            uint roleIdRef = 0;

            foreach (object child in children)
            {
                if (child is IAccessible)
                {
                    IAccessible accessibleChild = (IAccessible)child;
                    if (GetUIElementRole(accessibleChild, NativeMethods.CHILDID_SELF, ref roleIdRef) == "text")
                    {
                        windowDescription.MessageText += GetUIElementName(accessibleChild, NativeMethods.CHILDID_SELF);
                    }

                    GetPopupWindowMessageHelper(accessibleChild, ref windowDescription);
                }
            }
        }

        /// <summary>
        /// Finds the child by navigating into the logical hierarchy
        /// </summary>
        /// <param name="paccParent">Parent window as IAccessible</param>
        /// <param name="childId">ID of child window</param>
        /// <param name="childWindow">Child window description</param>
        /// <param name="ppaccChild">Child as IAccessible</param>
        /// <param name="foundId">ID found for child</param>
        /// <returns>flag if child found</returns>
        public static bool FindChild(IAccessible paccParent, int childId, PopupBasherConfiguration.PopupConfigChild childWindow, out IAccessible ppaccChild, out int foundId)
        {
            bool found = false;
            ppaccChild = null;
            foundId = 0;

            try
            {
                // First check if the parent is the search item
                found = IsMatching(paccParent, childId, childWindow);
                if (found)
                {
                    ppaccChild = paccParent;
                    foundId = childId;
                }
                else
                {
                    int numChildren = paccParent.accChildCount;
                    object[] children = new object[numChildren];
                    NativeMethods.AccessibleChildren(paccParent, 0, numChildren, children, out numChildren);

                    foreach (object child in children)
                    {
                        if (child is int)
                        {
                            // This is an element
                            found = IsMatching(paccParent, (int)child, childWindow);
                            if (found)
                            {
                                ppaccChild = paccParent;
                                foundId = (int)child;
                            }
                        }
                        else if (child is IAccessible)
                        {
                            found = FindChild((IAccessible)child, 0, childWindow, out ppaccChild, out foundId);
                        }

                        if (found)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, ex.ToString());
            }

            return found;
        }

        /// <summary>
        /// Checks if child window matches description in popup basher config
        /// </summary>
        /// <param name="acc">Parent as IAccessible</param>
        /// <param name="childId">ID of child window</param>
        /// <param name="childWindow">description of child window to look for</param>
        /// <returns>True if child matches description, false otherwise</returns>
        private static bool IsMatching(IAccessible acc, int childId, PopupBasherConfiguration.PopupConfigChild childWindow)
        {
            string classStr, nameStr, roleStr;
            uint roleRef = 0;
            bool found = false;
            IntPtr hwnd = (IntPtr)0;

            // First check if the parent is the search item
            roleStr = GetUIElementRole(acc, childId, ref roleRef);
            NativeMethods.WindowFromAccessibleObject(acc, ref hwnd);
            if ((uint)childWindow.Role == roleRef)
            {
                classStr = GetWindowClassForUIElement(acc);
                if ((childWindow.ClassName == null) || (String.Compare(classStr, childWindow.ClassName, true, CultureInfo.CurrentCulture) == 0))
                {
                    // If no name was specified, always treat this as a match
                    if (childWindow.Title == null)
                    {
                        found = true;
                    }
                    else
                    {
                        nameStr = GetUIElementName(acc, childId);
                        if (nameStr != null)
                        {
                            if (nameStr.Length > 0)
                            {
                                switch (childWindow.Search)
                                {
                                    case PopupBasherConfiguration.SearchMode.Exact:
                                        found = String.Compare(nameStr, childWindow.Title, true, CultureInfo.CurrentCulture) == 0;
                                        break;
                                    case PopupBasherConfiguration.SearchMode.RegEx:
                                        Regex r;

                                        r = new Regex(childWindow.Title, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                                        found = r.IsMatch(nameStr);
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return found;
        }
    }
}
