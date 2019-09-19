// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace TelepathyCommon.Registry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    ///     Simple registry watcher class that can be used to trigger discovery
    ///     if a reg key is updated.
    /// </summary>
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public sealed class WindowsRegistryWatcher<T> : IUpdateWatcher<T>, IDisposable
    {
        /// <summary>
        ///     lock to synchronize changes
        /// </summary>
        private readonly object changeLock = new object();

        /// <summary>
        ///     Registry root
        /// </summary>
        private readonly RegistryKey regKey;

        /// <summary>
        ///     The registry key name
        /// </summary>
        private readonly string regKeyName;

        /// <summary>
        ///     path of the registry key to watch
        /// </summary>
        private readonly string regKeyPath;

        /// <summary>
        ///     Root of the registry key to watch
        /// </summary>
        private readonly NativeMethods.HKEY_ROOT regKeyRoot;

        /// <summary>
        ///     The cached registry value
        /// </summary>
        private T cachedValue;

        private bool disposedValue; // To detect redundant calls

        /// <summary>
        ///     The event to wait for registry key update
        /// </summary>
        private AutoResetEvent notifyEvent;

        /// <summary>
        ///     Handle of the registry key to watch
        /// </summary>
        private NativeMethods.SafeRegistryHandle regKeyHandle;

        /// <summary>
        ///     Indicate if the registry value already exists
        /// </summary>
        private bool valueExist;

        /// <summary>
        ///     Wait handle for the event
        /// </summary>
        private RegisteredWaitHandle waitHandle;

        /// <summary>
        ///     Creates a new registry watcher.
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
            var paths = key.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (paths.Length == 0)
            {
                return;
            }

            var hive = paths[0];

            switch (hive.ToUpperInvariant())
            {
                case RegistryHiveString.LocalMachine:
                    this.regKeyRoot = NativeMethods.HKEY_ROOT.HKEY_LOCAL_MACHINE;
                    this.regKey = Registry.LocalMachine;
                    break;
                case RegistryHiveString.ClassesRoot:
                    this.regKeyRoot = NativeMethods.HKEY_ROOT.HKEY_CLASSES_ROOT;
                    this.regKey = Registry.ClassesRoot;
                    break;
                case RegistryHiveString.CurrentConfig:
                    this.regKeyRoot = NativeMethods.HKEY_ROOT.HKEY_CURRENT_CONFIG;
                    this.regKey = Registry.CurrentConfig;
                    break;
                case RegistryHiveString.PerformanceData:
                    this.regKeyRoot = NativeMethods.HKEY_ROOT.HKEY_PERFORMANCE_DATA;
                    this.regKey = Registry.PerformanceData;
                    break;
                case RegistryHiveString.Users:
                    this.regKeyRoot = NativeMethods.HKEY_ROOT.HKEY_USERS;
                    this.regKey = Registry.Users;
                    break;
                case RegistryHiveString.CurrentUser:
                    this.regKeyRoot = NativeMethods.HKEY_ROOT.HKEY_CURRENT_USER;
                    this.regKey = Registry.CurrentUser;
                    break;
            }

            // Create a default set event to trigger value initialization
            this.notifyEvent = new AutoResetEvent(true);

            // Register a wait on the event.
            this.waitHandle = ThreadPool.RegisterWaitForSingleObject(this.notifyEvent, this.OnRegKeyChanged, null, -1, false);
        }

        /// <summary>
        ///     Fired when the associated instance has been updated.
        /// </summary>
        public event EventHandler<RegistryValueChangedArgs<T>> InstanceUpdated;

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            this.Dispose(true);
        }

        internal static bool EqualCompare(T a, T b)
        {
            var t = typeof(T);
            if (t == typeof(byte[]))
            {
                return ((byte[])(object)a).SequenceEqual((byte[])(object)b);
            }

            if (t == typeof(string[]))
            {
                return ((string[])(object)a).SequenceEqual((string[])(object)b);
            }

            return EqualityComparer<T>.Default.Equals(a, b);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
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

                this.disposedValue = true;
            }
        }

        /// <summary>
        ///     Event callback registered to thread pool to be called
        /// </summary>
        /// <param name="state"></param>
        /// <param name="timedOut"></param>
        private void OnRegKeyChanged(object state, bool timedOut)
        {
            lock (this.changeLock)
            {
                this.RegisterForOneUpdate();
                this.RaiseKeyUpdated();
            }
        }

        /// <summary>
        ///     Raise the key update event to notify listeners
        /// </summary>
        private void RaiseKeyUpdated()
        {
            if (this.InstanceUpdated != null)
            {
                object newValueObj = null;
                using (var key = this.regKey.OpenSubKey(this.regKeyPath))
                {
                    if (key != null)
                    {
                        newValueObj = key.GetValue(this.regKeyName);
                    }
                }

                // sub key is deleted
                if (newValueObj == null)
                {
                    if (this.valueExist)
                    {
                        this.valueExist = false;
                        var tempCachedValue = this.cachedValue;
                        this.cachedValue = default;
                        this.InstanceUpdated(this, new RegistryValueChangedArgs<T>(RegistryValueChangedArgs<T>.ChangeType.Deleted, tempCachedValue, default));
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

                    if (this.valueExist)
                    {
                        if (!EqualCompare(newValue, this.cachedValue))
                        {
                            var tempCachedValue = this.cachedValue;
                            this.cachedValue = newValue;
                            this.InstanceUpdated(this, new RegistryValueChangedArgs<T>(RegistryValueChangedArgs<T>.ChangeType.Modified, tempCachedValue, newValue));
                        }
                    }

                    // sub key is created
                    else
                    {
                        this.valueExist = true;
                        this.cachedValue = newValue;
                        this.InstanceUpdated(this, new RegistryValueChangedArgs<T>(RegistryValueChangedArgs<T>.ChangeType.Created, default, newValue));
                    }
                }
            }
        }

        /// <summary>
        ///     Register the event to listen on one update on the registry key
        /// </summary>
        private void RegisterForOneUpdate()
        {
            if (this.regKeyHandle != null)
            {
                this.regKeyHandle.Close();
                this.regKeyHandle = null;
            }

            // Open the reg key
            // Use UIntPtr here because there will be overflowException by cast uint to IntPtr directly int x86 platform
            // UIntPtr cause CLS-complain issue, but sdm will be only called by c#
            var result = NativeMethods.RegOpenKeyEx((UIntPtr)this.regKeyRoot, this.regKeyPath, 0, NativeMethods.KEY_READ, out this.regKeyHandle);
            if (result != NativeMethods.ERROR_SUCCESS)

                // The watcher is stopped from this point. Ideally we should notify update engine to create 
            {
                // a new watcher or log a message.
                return;
            }

            // Listen for changes.
            result = NativeMethods.RegNotifyChangeKeyValue(
                this.regKeyHandle,
                true,
                NativeMethods.REG_NOTIFY_CHANGE_NAME | NativeMethods.REG_NOTIFY_CHANGE_LAST_SET,
                this.notifyEvent.SafeWaitHandle,
                true);

            if (result != NativeMethods.ERROR_SUCCESS)

                // The watcher is stopped from this point. Ideally we should notify update engine to create 
            {
                // a new watcher or log a message.
            }
        }
    }

    /// <summary>
    ///     Update watcher interface implemented by managers that
    ///     provide notifications of updated to the modeled instance.
    /// </summary>
    public interface IUpdateWatcher<T>
    {
        /// <summary>
        ///     Signals that the associated instance has been updated.
        /// </summary>
        event EventHandler<RegistryValueChangedArgs<T>> InstanceUpdated;
    }

    /// <summary>
    ///     The registry root const strings
    /// </summary>
    public static class RegistryHiveString
    {
        public const string ClassesRoot = "HKEY_CLASSES_ROOT";

        public const string CurrentConfig = "HKEY_CURRENT_CONFIG";

        public const string CurrentUser = "HKEY_CURRENT_USER";

        public const string DynData = "HKEY_DYN_DATA";

        public const string LocalMachine = "HKEY_LOCAL_MACHINE";

        public const string PerformanceData = "HKEY_PERFORMANCE_DATA";

        public const string Users = "HKEY_USERS";
    }

    internal static class NativeMethods
    {
        /// <summary>
        ///     Note: Officially -1 is the recommended invalid handle value for
        ///     registry keys, but we'll also get back 0 as an invalid handle from
        ///     RegOpenKeyEx.
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
                this.SetHandle(handle);
            }

            protected override bool ReleaseHandle()
            {
                // Returns a Win32 error code, ERROR_SUCCESS for success
                return RegCloseKey(this.handle) == ERROR_SUCCESS;
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

        internal const int KEY_READ = (STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY) & ~SYNCHRONIZE;

        internal enum HKEY_ROOT : uint
        {
            HKEY_CLASSES_ROOT = 0x80000000,

            HKEY_CURRENT_USER = 0x80000001,

            HKEY_LOCAL_MACHINE = 0x80000002,

            HKEY_USERS = 0x80000003,

            HKEY_PERFORMANCE_DATA = 0x80000004,

            HKEY_CURRENT_CONFIG = 0x80000005,

            HKEY_DYN_DATA = 0x80000006
        }

        internal const int REG_NOTIFY_CHANGE_NAME = 1;

        internal const int REG_NOTIFY_CHANGE_LAST_SET = 4;

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern int RegCloseKey(IntPtr hKey);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern int RegNotifyChangeKeyValue(
            SafeRegistryHandle hKey,
            [MarshalAs(UnmanagedType.Bool)] bool watchSubTree,
            uint notifyFilter,
            SafeWaitHandle regEvent,
            [MarshalAs(UnmanagedType.Bool)] bool async);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false, SetLastError = true)]
        internal static extern int RegOpenKeyEx(UIntPtr hKey, string lpSubKey, int ulOptions, int samDesired, out SafeRegistryHandle hkResult);
    }
}