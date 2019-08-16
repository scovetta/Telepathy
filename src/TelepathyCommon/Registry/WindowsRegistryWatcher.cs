//-------------------------------------------------------------------------------------------------
// <copyright file="WindowsRegistryWatcher.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     Simple registry watcher class that can be used to trigger discovery 
//     if a reg key is updated.
// </summary>
//-------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace TelepathyCommon.Registry
{
    /// <summary>
    /// Simple registry watcher class that can be used to trigger discovery 
    /// if a reg key is updated.
    /// </summary>
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public sealed class WindowsRegistryWatcher<T> : IUpdateWatcher<T>, IDisposable
    {
        /// <summary>
        /// Root of the registry key to watch
        /// </summary>
        private NativeMethods.HKEY_ROOT regKeyRoot;

        /// <summary>
        /// Registry root
        /// </summary>
        private RegistryKey regKey;

        /// <summary>
        /// path of the registry key to watch
        /// </summary>
        private string regKeyPath;

        /// <summary>
        /// The registry key name
        /// </summary>
        private string regKeyName;

        /// <summary>
        /// Handle of the registry key to watch
        /// </summary>
        private NativeMethods.SafeRegistryHandle regKeyHandle;

        /// <summary>
        /// The event to wait for registry key update
        /// </summary>
        private AutoResetEvent notifyEvent;

        /// <summary>
        /// Wait handle for the event
        /// </summary>
        private RegisteredWaitHandle waitHandle;

        /// <summary>
        /// The cached registry value
        /// </summary>
        private T cachedValue = default(T);

        /// <summary>
        /// Indicate if the registry value already exists
        /// </summary>
        private bool valueExist = false;

        /// <summary>
        /// lock to synchronize changes
        /// </summary>
        private object changeLock = new object();

        /// <summary>
        /// Creates a new registry watcher.
        /// </summary>
        /// <param name="hive">The hive that contains the key</param>
        /// <param name="key">The path to the key to watch</param>
        /// <param name="instanceId">The associated instance that should be discovered.</param>
        public WindowsRegistryWatcher(string key, string name)
        {
            this.regKeyName = name;
            if (!key.Contains("\\"))
            {
                return;
            }
            this.regKeyPath = key.Substring(key.IndexOf('\\') + 1);
            string[] paths = key.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (paths.Length == 0)
            {
                return;
            }
            string hive = paths[0];

            switch (hive.ToUpperInvariant())
            {
                case RegistryHiveString.LocalMachine:
                    this.regKeyRoot = NativeMethods.HKEY_ROOT.HKEY_LOCAL_MACHINE;
                    this.regKey = Microsoft.Win32.Registry.LocalMachine;
                    break;
                case RegistryHiveString.ClassesRoot:
                    this.regKeyRoot = NativeMethods.HKEY_ROOT.HKEY_CLASSES_ROOT;
                    this.regKey = Microsoft.Win32.Registry.ClassesRoot;
                    break;
                case RegistryHiveString.CurrentConfig:
                    this.regKeyRoot = NativeMethods.HKEY_ROOT.HKEY_CURRENT_CONFIG;
                    this.regKey = Microsoft.Win32.Registry.CurrentConfig;
                    break;
                case RegistryHiveString.PerformanceData:
                    this.regKeyRoot = NativeMethods.HKEY_ROOT.HKEY_PERFORMANCE_DATA;
                    this.regKey = Microsoft.Win32.Registry.PerformanceData;
                    break;
                case RegistryHiveString.Users:
                    this.regKeyRoot = NativeMethods.HKEY_ROOT.HKEY_USERS;
                    this.regKey = Microsoft.Win32.Registry.Users;
                    break;
                case RegistryHiveString.CurrentUser:
                    this.regKeyRoot = NativeMethods.HKEY_ROOT.HKEY_CURRENT_USER;
                    this.regKey = Microsoft.Win32.Registry.CurrentUser;
                    break;
                default:
                    break;
            }

            // Create a default set event to trigger value initialization
            this.notifyEvent = new AutoResetEvent(true);
            // Register a wait on the event.
            this.waitHandle = ThreadPool.RegisterWaitForSingleObject(notifyEvent, OnRegKeyChanged, null, -1, false);

        }

        /// <summary>
        /// Fired when the associated instance has been updated.
        /// </summary>
        public event EventHandler<RegistryValueChangedArgs<T>> InstanceUpdated;

        /// <summary>
        /// Raise the key update event to notify listeners
        /// </summary>
        private void RaiseKeyUpdated()
        {
            if (this.InstanceUpdated != null)
            {
                object newValueObj = null;
                using (RegistryKey key = this.regKey.OpenSubKey(this.regKeyPath))
                {
                    if (key != null)
                    {
                        newValueObj = key.GetValue(this.regKeyName);
                    }
                }

                // sub key is deleted
                if (newValueObj == null)
                {
                    if (valueExist)
                    {
                        valueExist = false;
                        T tempCachedValue = cachedValue;
                        cachedValue = default(T);
                        this.InstanceUpdated(this, new RegistryValueChangedArgs<T>(RegistryValueChangedArgs<T>.ChangeType.Deleted, tempCachedValue, default(T)));
                    }
                }
                // sub key is updated
                else
                {
                    T newValue;
                    if (typeof(T) == typeof(Guid))
                    {
                        newValue = (T)(object)Guid.Parse(newValueObj.ToString());
                    }
                    else
                    {
                        newValue = (T)newValueObj;
                    }

                    if (valueExist)
                    {
                        if (!EqualCompare(newValue, cachedValue))
                        {
                            T tempCachedValue = cachedValue;
                            cachedValue = newValue;
                            this.InstanceUpdated(this, new RegistryValueChangedArgs<T>(RegistryValueChangedArgs<T>.ChangeType.Modified, tempCachedValue, newValue));
                        }
                    }
                    // sub key is created
                    else
                    {
                        valueExist = true;
                        cachedValue = newValue;
                        this.InstanceUpdated(this, new RegistryValueChangedArgs<T>(RegistryValueChangedArgs<T>.ChangeType.Created, default(T), newValue));
                    }
                }
            }
        }

        /// <summary>
        /// Event callback registered to thread pool to be called 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="timedOut"></param>
        private void OnRegKeyChanged(object state, bool timedOut)
        {
            lock (this.changeLock)
            {
                RegisterForOneUpdate();
                RaiseKeyUpdated();
            }
        }

        /// <summary>
        /// Register the event to listen on one update on the registry key
        /// </summary>
        private void RegisterForOneUpdate()
        {
            if (regKeyHandle != null)
            {
                regKeyHandle.Close();
                regKeyHandle = null;
            }

            // Open the reg key
            // Use UIntPtr here because there will be overflowException by cast uint to IntPtr directly int x86 platform
            // UIntPtr cause CLS-complain issue, but sdm will be only called by c#
            int result = NativeMethods.RegOpenKeyEx((UIntPtr)regKeyRoot, regKeyPath, 0, NativeMethods.KEY_READ, out regKeyHandle);
            if (result != NativeMethods.ERROR_SUCCESS)
            {
                // The watcher is stopped from this point. Ideally we should notify update engine to create 
                // a new watcher or log a message.
                return;
            }

            // Listen for changes.
            result = NativeMethods.RegNotifyChangeKeyValue(
                    regKeyHandle,
                    true,
                    NativeMethods.REG_NOTIFY_CHANGE_NAME | NativeMethods.REG_NOTIFY_CHANGE_LAST_SET,
                    notifyEvent.SafeWaitHandle,
                    true);

            if (result != NativeMethods.ERROR_SUCCESS)
            {
                // The watcher is stopped from this point. Ideally we should notify update engine to create 
                // a new watcher or log a message.
                return;
            }
        }

        internal static bool EqualCompare(T a, T b)
        {
            Type t = typeof(T);
            if (t == typeof(byte[]))
            {
                return Enumerable.SequenceEqual<byte>((byte[])(object)a, (byte[])(object)b);
            }
            else if (t == typeof(string[]))
            {
                return Enumerable.SequenceEqual<string>((string[])(object)a, (string[])(object)b);
            }
            else
            {
                return EqualityComparer<T>.Default.Equals(a, b);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.waitHandle != null)
                    {
                        this.waitHandle.Unregister(null);
                        this.waitHandle = null;
                    }

                    if (this.notifyEvent != null)
                    {
                        this.notifyEvent.Dispose();
                        this.notifyEvent = null;
                    }

                    if (this.regKeyHandle != null)
                    {
                        this.regKeyHandle.Dispose();
                        this.regKeyHandle = null;
                    }
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    /// <summary>
    /// Update watcher interface implemented by managers that 
    /// provide notifications of updated to the modeled instance.
    /// </summary>
    public interface IUpdateWatcher<T>
    {
        /// <summary>
        /// Signals that the associated instance has been updated.
        /// </summary>
        event EventHandler<RegistryValueChangedArgs<T>> InstanceUpdated;
    }

    /// <summary>
    /// The registry root const strings
    /// </summary>
    public static class RegistryHiveString
    {
        public const string ClassesRoot = "HKEY_CLASSES_ROOT";
        public const string CurrentUser = "HKEY_CURRENT_USER";
        public const string LocalMachine = "HKEY_LOCAL_MACHINE";
        public const string Users = "HKEY_USERS";
        public const string PerformanceData = "HKEY_PERFORMANCE_DATA";
        public const string CurrentConfig = "HKEY_CURRENT_CONFIG";
        public const string DynData = "HKEY_DYN_DATA";
    }

    internal static class NativeMethods
    {
        /// <summary>
        /// Note: Officially -1 is the recommended invalid handle value for
        /// registry keys, but we'll also get back 0 as an invalid handle from
        /// RegOpenKeyEx.
        /// </summary>
        internal class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            internal SafeRegistryHandle()
                : base(true)
            {
            }

            internal SafeRegistryHandle(IntPtr handle)
                : base(true)
            {
                base.SetHandle(handle);
            }

            override protected bool ReleaseHandle()
            {
                // Returns a Win32 error code, ERROR_SUCCESS for success
                return NativeMethods.RegCloseKey(handle) == NativeMethods.ERROR_SUCCESS;
            }
        }

        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        internal const int ERROR_SUCCESS = 0;
        internal const int KEY_QUERY_VALUE = 0x0001;
        internal const int KEY_ENUMERATE_SUB_KEYS = 0x0008;
        internal const int KEY_NOTIFY = 0x0010;
        internal const int READ_CONTROL = 0x00020000;
        internal const int STANDARD_RIGHTS_READ = READ_CONTROL;
        internal const int SYNCHRONIZE = 0x00100000;

        internal const int KEY_READ = ((STANDARD_RIGHTS_READ |
                                        KEY_QUERY_VALUE |
                                        KEY_ENUMERATE_SUB_KEYS |
                                        KEY_NOTIFY)
                                        &
                                        (~SYNCHRONIZE));

        internal enum HKEY_ROOT : uint
        {
            HKEY_CLASSES_ROOT = 0x80000000,
            HKEY_CURRENT_USER = 0x80000001,
            HKEY_LOCAL_MACHINE = 0x80000002,
            HKEY_USERS = 0x80000003,
            HKEY_PERFORMANCE_DATA = 0x80000004,
            HKEY_CURRENT_CONFIG = 0x80000005,
            HKEY_DYN_DATA = 0x80000006,
        }

        internal const int REG_NOTIFY_CHANGE_NAME = 1;
        internal const int REG_NOTIFY_CHANGE_LAST_SET = 4;

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern int RegCloseKey(IntPtr hKey);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal extern static int RegNotifyChangeKeyValue(SafeRegistryHandle hKey, [MarshalAs(UnmanagedType.Bool)]bool watchSubTree, uint notifyFilter, SafeWaitHandle regEvent, [MarshalAs(UnmanagedType.Bool)]bool async);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, SetLastError = true)]
        internal extern static int RegOpenKeyEx(UIntPtr hKey, string lpSubKey, int ulOptions, int samDesired, out SafeRegistryHandle hkResult);

    }
}
