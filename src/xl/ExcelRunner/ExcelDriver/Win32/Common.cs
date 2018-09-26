//------------------------------------------------------------------------------
// <copyright file="Common.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Common data for Win32 API calls
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel.Win32
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using Accessibility;

    /// <summary>
    /// Callback delegate which accepts results from user32 functions
    /// </summary>
    /// <param name="hwnd">Window handle</param>
    /// <param name="lParam">Enumeration index</param>
    /// <returns>Success or failure</returns>
    internal delegate bool EnumWindowsCallBack(IntPtr hwnd, IntPtr lParam);

    /// <summary>
    /// Callback delegate which accepts windows events 
    /// </summary>
    /// <param name="hWinEventHook">Handle to the windows event hook</param>
    /// <param name="accEvent">Event reference</param>
    /// <param name="hwnd">Window handle</param>
    /// <param name="idObject">object identifier</param>
    /// <param name="idChild">child element identifier</param>
    /// <param name="dwEventThread">Thread handling event</param>
    /// <param name="dwmsEventTime">Time of event</param>
    internal delegate void WinEventDelegate(IntPtr hWinEventHook, AccessibleEvents accEvent, IntPtr hwnd, uint idObject, uint idChild, uint dwEventThread, uint dwmsEventTime);

    /// <summary>
    /// Enumerations of message IDs
    /// </summary>
    internal enum Messages : uint
    {
        WM_NULL = 0x0000,
        WM_CREATE = 0x0001,
        WM_DESTROY = 0x0002,
        WM_MOVE = 0x0003,
        WM_SIZE = 0x0005,
        WM_ACTIVATE = 0x0006,
        WM_SETFOCUS = 0x0007,
        WM_KILLFOCUS = 0x0008,
        WM_ENABLE = 0x000A,
        WM_SETREDRAW = 0x000B,
        WM_SETTEXT = 0x000C,
        WM_GETTEXT = 0x000D,
        WM_GETTEXTLENGTH = 0x000E,
        WM_PAINT = 0x000F,
        WM_CLOSE = 0x0010,
        WM_QUIT = 0x0012,
        WM_ERASEBKGND = 0x0014,
        WM_SYSCOLORCHANGE = 0x0015,
        WM_SHOWWINDOW = 0x0018,
        WM_WININICHANGE = 0x001A,
        WM_SETTINGCHANGE = WM_WININICHANGE,
        WM_DEVMODECHANGE = 0x001B,
        WM_ACTIVATEAPP = 0x001C,
        WM_FONTCHANGE = 0x001D,
        WM_TIMECHANGE = 0x001E,
        WM_CANCELMODE = 0x001F,
        WM_SETCURSOR = 0x0020,
        WM_MOUSEACTIVATE = 0x0021,
        WM_CHILDACTIVATE = 0x0022,
        WM_QUEUESYNC = 0x0023,
        WM_GETMINMAXINFO = 0x0024,
        WM_KEYFIRST = 0x0100,
        WM_KEYDOWN = 0x0100,
        WM_KEYUP = 0x0101,
        WM_CHAR = 0x0102,
        WM_DEADCHAR = 0x0103,
        WM_SYSKEYDOWN = 0x0104,
        WM_SYSKEYUP = 0x0105,
        WM_SYSCHAR = 0x0106,
        WM_SYSDEADCHAR = 0x0107,
        BM_CLICK = 0x00F5
    }

    /// <summary>
    /// Enumerations of timeout codes
    /// </summary>
    internal enum SendMessageTimeoutCode : uint
    {
        SMTO_NORMAL = 0x0000,
        SMTO_BLOCK = 0x0001,
        SMTO_ABORTIFHUNG = 0x0002
    }

    /// <summary>
    /// Enumeration of button control messages
    /// </summary>
    internal enum ButtonControlMessages : uint
    {
        BM_GETCHECK = 0x00F0,
        BM_SETCHECK = 0x00F1,
        BM_GETSTATE = 0x00F2,
        BM_SETSTATE = 0x00F3,
        BM_SETSTYLE = 0x00F4,
        BM_CLICK = 0x00F5,
        BM_GETIMAGE = 0x00F6,
        BM_SETIMAGE = 0x00F7,
        BST_UNCHECKED = 0x0000,
        BST_CHECKED = 0x0001,
        BST_INDETERMINATE = 0x0002,
        BST_PUSHED = 0x0004,
        BST_FOCUS = 0x0008
    }

    /// <summary>
    /// Enumeration of key events
    /// </summary>
    internal enum KeyFlags : uint
    {
        KF_EXTENDED = 0x0100,
        KF_DLGMODE = 0x0800,
        KF_MENUMODE = 0x1000,
        KF_ALTDOWN = 0x2000,
        KF_REPEAT = 0x4000,
        KF_UP = 0x8000
    }

    /// <summary>
    /// Enumeration of windows hooks
    /// </summary>
    internal enum WindowsHook : uint
    {
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7
    }

    /// <summary>
    /// Enumeration of windows events
    /// </summary>
    internal enum WinEvent : uint
    {
        WINEVENT_OUTOFCONTEXT = 0x0000,  // Events are ASYNC
        WINEVENT_SKIPOWNTHREAD = 0x0001,  // Don't call back for events on installer's thread
        WINEVENT_SKIPOWNPROCESS = 0x0002,  // Don't call back for events on installer's process
        WINEVENT_INCONTEXT = 0x0004  // Events are SYNC, this causes your dll to be injected into every process
    }

    /// <summary>
    /// Enumeration of system state flags
    /// </summary>
    internal enum StateSystemFlags : uint
    {
        STATE_SYSTEM_UNAVAILABLE = 0x00000001,  // Disabled
        STATE_SYSTEM_SELECTED = 0x00000002,
        STATE_SYSTEM_FOCUSED = 0x00000004,
        STATE_SYSTEM_PRESSED = 0x00000008,
        STATE_SYSTEM_CHECKED = 0x00000010,
        STATE_SYSTEM_MIXED = 0x00000020,  // 3-state checkbox or toolbar button
        STATE_SYSTEM_INDETERMINATE = STATE_SYSTEM_MIXED,
        STATE_SYSTEM_READONLY = 0x00000040,
        STATE_SYSTEM_HOTTRACKED = 0x00000080,
        STATE_SYSTEM_DEFAULT = 0x00000100,
        STATE_SYSTEM_EXPANDED = 0x00000200,
        STATE_SYSTEM_COLLAPSED = 0x00000400,
        STATE_SYSTEM_BUSY = 0x00000800,
        STATE_SYSTEM_FLOATING = 0x00001000,  // Children "owned" not "contained" by parent
        STATE_SYSTEM_MARQUEED = 0x00002000,
        STATE_SYSTEM_ANIMATED = 0x00004000,
        STATE_SYSTEM_INVISIBLE = 0x00008000,
        STATE_SYSTEM_OFFSCREEN = 0x00010000,
        STATE_SYSTEM_SIZEABLE = 0x00020000,
        STATE_SYSTEM_MOVEABLE = 0x00040000,
        STATE_SYSTEM_SELFVOICING = 0x00080000,
        STATE_SYSTEM_FOCUSABLE = 0x00100000,
        STATE_SYSTEM_SELECTABLE = 0x00200000,
        STATE_SYSTEM_LINKED = 0x00400000,
        STATE_SYSTEM_TRAVERSED = 0x00800000,
        STATE_SYSTEM_MULTISELECTABLE = 0x01000000,  // Supports multiple selection
        STATE_SYSTEM_EXTSELECTABLE = 0x02000000,  // Supports extended selection
        STATE_SYSTEM_ALERT_LOW = 0x04000000,  // This information is of low priority
        STATE_SYSTEM_ALERT_MEDIUM = 0x08000000,  // This information is of medium priority
        STATE_SYSTEM_ALERT_HIGH = 0x10000000,  // This information is of high priority
        STATE_SYSTEM_PROTECTED = 0x20000000,  // access to this is restricted
        STATE_SYSTEM_VALID = 0x3FFFFFFF
    }

    /// <summary>
    /// Enumerations of object identifiers
    /// </summary>
    internal enum ObjIdentifier : uint
    {
        OBJID_WINDOW = 0x00000000,
        OBJID_SYSMENU = 0xFFFFFFFF,
        OBJID_TITLEBAR = 0xFFFFFFFE,
        OBJID_MENU = 0xFFFFFFFD,
        OBJID_CLIENT = 0xFFFFFFFC,
        OBJID_VSCROLL = 0xFFFFFFFB,
        OBJID_HSCROLL = 0xFFFFFFFA,
        OBJID_SIZEGRIP = 0xFFFFFFF9,
        OBJID_CARET = 0xFFFFFFF8,
        OBJID_CURSOR = 0xFFFFFFF7,
        OBJID_ALERT = 0xFFFFFFF6,
        OBJID_SOUND = 0xFFFFFFF5,
        OBJID_QUERYCLASSNAMEIDX = 0xFFFFFFF4,
        OBJID_NATIVEOM = 0xFFFFFFF0
    }

    /// <summary>
    /// Enumeration of window display modes
    /// </summary>
    internal enum ShowWindowMode : uint
    {
        SW_HIDE = 0,
        SW_SHOWNORMAL = 1,
        SW_NORMAL = 1,
        SW_SHOWMINIMIZED = 2,
        SW_SHOWMAXIMIZED = 3,
        SW_MAXIMIZE = 3,
        SW_SHOWNOACTIVATE = 4,
        SW_SHOW = 5,
        SW_MINIMIZE = 6,
        SW_SHOWMINNOACTIVE = 7,
        SW_SHOWNA = 8,
        SW_RESTORE = 9,
        SW_SHOWDEFAULT = 10,
        SW_FORCEMINIMIZE = 11,
        SW_MAX = 11
    }

    /// <summary>
    /// Class exposing all the P/Invoke functions
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// Minimum event identifier
        /// </summary>
        public const uint EVENT_MIN = 0x00000001;

        /// <summary>
        /// Maximum event identifier
        /// </summary>
        public const uint EVENT_MAX = 0x7FFFFFFF;

        /// <summary>
        /// ID to use for self
        /// </summary>
        public const int CHILDID_SELF = 0;

        /// <summary>
        /// Interface ID of IAccessible interface
        /// </summary>
        public const string IID_IAccessible = "{618736E0-3C3D-11CF-810C-00AA00389B71}";

        /// <summary>
        /// Callback definition for use with EnumWindows Win32 API
        /// </summary>
        /// <param name="hwnd">Handle to Window</param>
        /// <param name="lParam">User provided integer</param>
        /// <returns>Continuation status </returns>
        public delegate bool CallBackPtr(int hwnd, int lParam);

        /// <summary>
        /// Retrieves the name of the class to which the specified window belongs. 
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs. </param>
        /// <param name="lpClassName">The class name string. </param>
        /// <param name="nMaxCount">The length, in characters, of the buffer pointed to by the lpClassName parameter. The class name string is truncated if it is longer than the buffer and is always null-terminated. </param>
        /// <returns>If the function succeeds, the return value is the number of characters copied to the specified buffer. If the function fails, the return value is zero.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, [Out] StringBuilder lpClassName, int nMaxCount);

        /// <summary>
        /// Retrieves a handle to a window whose class name and window name match the specified strings. 
        /// </summary>
        /// <param name="hWndParent">A handle to the parent window whose child windows are to be searched.</param>
        /// <param name="hWndChildAfter">A handle to a child window. The search begins with the next child window in the Z order. The child window must be a direct child window of hwndParent, not just a descendant window. </param>
        /// <param name="szClass">The class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx function</param>
        /// <param name="szWindow">The window name (the window's title). If this parameter is NULL, all window names match. </param>
        /// <returns>If the function succeeds, the return value is a handle to the window that has the specified class and window names.</returns>
        [DllImport("user32.dll", CharSet=CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string szClass, string szWindow);

        /// <summary>
        /// Sets the keyboard focus to the specified window. The window must be attached to the calling thread's message queue. 
        /// </summary>
        /// <param name="hWnd">A handle to the window that will receive the keyboard input.</param>
        /// <returns>If the function succeeds, the return value is the handle to the window that previously had the keyboard focus. If the hWnd parameter is invalid or the window is not attached to the calling thread's message queue, the return value is NULL.</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        /// <summary>
        /// Sets the specified window's show state. 
        /// </summary>
        /// <param name="hWnd">A handle to the window. </param>
        /// <param name="nCmdShow">Controls how the window is to be shown.</param>
        /// <returns>If the window was previously visible, the return value is nonzero. </returns>
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowMode nCmdShow);

        /// <summary>
        /// Retrieves the address of the specified interface for the object associated with the specified window.
        /// </summary>
        /// <param name="hwnd">Specifies the handle of a window for which an object is to be retrieved. To retrieve an interface pointer to the cursor or caret object, specify NULL and use the appropriate object ID in dwObjectID.</param>
        /// <param name="dwObjectID">Specifies the object ID. This value is one of the standard object identifier constants or a custom object ID such as OBJID_NATIVEOM, which is the object ID for the Office native object model. For more information about OBJID_NATIVEOM, see the Remarks section in this topic.</param>
        /// <param name="riid">Specifies the reference identifier of the requested interface. This value is either IID_IAccessible or IID_IDispatch, but it can also be IID_IUnknown, or the IID of any interface that the object is expected to support.</param>
        /// <param name="ppacc">Address of a pointer variable that receives the address of the specified interface.</param>
        /// <returns>If successful, returns S_OK. Otherwise returns error.</returns>
        [DllImport("oleacc.dll")]
        public static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint dwObjectID, ref Guid riid, out IAccessible ppacc);

        /// <summary>
        /// Retrieves the child ID or IDispatch of each child within an accessible container object.
        /// </summary>
        /// <param name="paccContainer">Pointer to the container object's IAccessible interface.</param>
        /// <param name="iChildStart">Specifies the zero-based index of the first child that is retrieved. This parameter is an index, not a child ID, and it is usually is set to zero (0).</param>
        /// <param name="cChildren">Specifies the number of children to retrieve. To retrieve the current number of children, an application calls IAccessible::get_accChildCount.</param>
        /// <param name="rgvarChildren">Pointer to an array of VARIANT structures structures that receives information about the container's children. If the vt member of an array element is VT_I4, then the lVal member for that element is the child ID. If the vt member of an array element is VT_DISPATCH, then the pdispVal member for that element is the address of the child object's IDispatch interface.</param>
        /// <param name="pcObtained">Address of a variable that receives the number of elements in the rgvarChildren array that is populated by the AccessibleChildren function. This value is the same as that of the cChildren parameter; however, if you request more children than exist, this value will be less than that of cChildren.</param>
        /// <returns>If successful, returns S_OK. Otherwise returns error.</returns>
        [DllImport("oleacc.dll")]
        public static extern uint AccessibleChildren(IAccessible paccContainer, int iChildStart, int cChildren, [Out] object[] rgvarChildren, out int pcObtained);

        /// <summary>
        /// Retrieves the window handle that corresponds to a particular instance of an IAccessible interface.
        /// </summary>
        /// <param name="pacc">Pointer to the IAccessible interface whose corresponding window handle will be retrieved. This parameter must not be NULL.</param>
        /// <param name="phwnd">Address of a variable that receives a handle to the window containing the object specified in pacc. If this value is NULL after the call, the object is not contained within a window; for example, the mouse pointer is not contained within a window.</param>
        /// <returns>If successful, returns S_OK. Otherwise returns error.</returns>
        [DllImport("oleacc.dll")]
        public static extern uint WindowFromAccessibleObject(IAccessible pacc, ref IntPtr phwnd);

        /// <summary>
        /// Retrieves the localized string that describes the object's role for the specified role value.
        /// </summary>
        /// <param name="dwRole">One of the object role constants.</param>
        /// <param name="lpszRole">Address of a buffer that receives the role text string. If this parameter is NULL, the function returns the role string's length, not including the null character.</param>
        /// <param name="cchRoleMax">The size of the buffer that is pointed to by the lpszRole parameter. For ANSI strings, this value is measured in bytes; for Unicode strings, it is measured in characters.</param>
        /// <returns>If successful, returns S_OK. Otherwise returns error.</returns>
        [DllImport("oleacc.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetRoleText(uint dwRole, [Out] StringBuilder lpszRole, uint cchRoleMax);

        /// <summary>
        /// Helper method to bring focus to a specified window
        /// </summary>
        /// <param name="hwnd"> pointer to window </param>
        /// <returns> success or failure </returns>
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        /// <summary>
        /// Helper method to retrieve the window that currently has focus
        /// </summary>
        /// <returns> Pointer to window </returns>
        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Get ThreadID from window handle. Overload to use when you don't care about the process ID.
        /// </summary>
        /// <param name="hWnd">handle to window</param>
        /// <param name="ProcessId">id of process running window</param>
        /// <returns>ID of thread running window</returns>
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        /// <summary>
        /// Get ThreadID from window handle. Overload to use when you care about the process ID.
        /// </summary>
        /// <param name="hWnd">handle to window</param>
        /// <param name="lpdwProcessId">id of process running window</param>
        /// <returns>ID of thread running window</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// Get current unmanaged Thread ID
        /// </summary>
        /// <returns> ID of current thread </returns>
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        /// <summary>
        /// Enumerates all top-level windows on the screen by passing the handle to each window, in turn, to an application-defined callback function
        /// </summary>
        /// <param name="callPtr">A pointer to an application-defined callback function</param>
        /// <param name="lPar">An application-defined value to be passed to the callback function. </param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero </returns>
        [DllImport("user32.dll")]
        public static extern int EnumWindows(CallBackPtr callPtr, int lPar);
    }
}
