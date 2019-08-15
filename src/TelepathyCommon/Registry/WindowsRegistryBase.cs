using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace TelepathyCommon.Registry
{
    public abstract class WindowsRegistryBase : RegistryProperty, IDisposable
    {
        private List<IDisposable> watchers = new List<IDisposable>();
        protected abstract RegistryKey CreateOrOpenSubKey(string key);

        private static readonly Task CompletedTask =
#if net40
            TaskEx.FromResult(0);
#else
            Task.FromResult(0);
#endif

        public override Task<T> GetValueAsync<T>(string key, string name, CancellationToken token, T defaultValue = default(T))
        {
            T res;
            if (typeof(T) == typeof(Guid))
            {
                var guidStr = (string)Microsoft.Win32.Registry.GetValue(key, name, string.Empty);
                if (string.IsNullOrEmpty(guidStr))
                {
                    res = (T)(object)Guid.Empty;
                }
                else
                {
                    res = (T)(object)Guid.Parse(guidStr);
                }
            }
            else
            {
                var value = Microsoft.Win32.Registry.GetValue(key, name, defaultValue);
                if (value == null)
                {
                    res = defaultValue;
                }
                else
                {
                    res = (T)value;
                }
            }

#if net40
            return TaskEx.FromResult(res);
#else
            return Task.FromResult(res);
#endif
        }

        public override Task SetValueAsync<T>(string key, string name, T value, CancellationToken token)
        {
            using (var regKey = this.CreateOrOpenSubKey(key))
            {
                if (regKey == null)
                {
                    //error handling
                    throw new InvalidOperationException(string.Format("The registry key {0} under {1} is null", name, key));
                }
                else
                {
                    regKey.SetValue(name, value);
                }
            }

            return CompletedTask;
        }

        public override Task DeleteValueAsync(string key, string name, CancellationToken token)
        {
            using (var regKey = this.CreateOrOpenSubKey(key))
            {
                if (regKey == null)
                {
                    //error handling
                    throw new InvalidOperationException(string.Format("The registry key {0} under {1} is null", name, key));
                }
                else
                {
                    regKey.DeleteValue(name);
                }
            }

            return CompletedTask;
        }

        /// <summary>
        /// Register a callback when the value identified by the key and name is created, changed or deleted.
        /// The first time you register, you will get a value created event always.
        /// </summary>
        /// <typeparam name="T">The value type</typeparam>
        /// <param name="key">the key</param>
        /// <param name="name">the name.</param>
        /// <param name="checkPeriod">the check period.</param>
        /// <param name="callback">the callback.</param>
        /// <param name="token">cancel this token to cancel the registration.</param>
        /// <returns>The task which is running during the whole monitoring process. Exceptions happened during this process is carried back by the task.</returns>
        public override Task MonitorRegistryKeyAsync<T>(string key, string name, TimeSpan checkPeriod, EventHandler<RegistryValueChangedArgs<T>> callback, CancellationToken token)
        {
            WindowsRegistryWatcher<T> watcher = new WindowsRegistryWatcher<T>(key, name);
            watcher.InstanceUpdated += callback;
            watchers.Add(watcher);
            return CompletedTask;
        }
        
        public RegistryKey GetRootKey(string rootKeyName)
        {
            switch (rootKeyName)
            {
                case "HKEY_CURRENT_USER":
                    return Microsoft.Win32.Registry.CurrentUser;

                case "HKEY_LOCAL_MACHINE":
                    return Microsoft.Win32.Registry.LocalMachine;

                case "HKEY_CLASSES_ROOT":
                    return Microsoft.Win32.Registry.ClassesRoot;

                case "HKEY_USERS":
                    return Microsoft.Win32.Registry.Users;

                case "HKEY_PERFORMANCE_DATA":
                    return Microsoft.Win32.Registry.PerformanceData;

                case "HKEY_CURRENT_CONFIG":
                    return Microsoft.Win32.Registry.CurrentConfig;

                case "HKEY_DYN_DATA":
                    return Microsoft.Win32.Registry.CurrentConfig;
            }

            return null;
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var watcher in this.watchers)
                    {
                        watcher.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);

            // Suppress finalization of this disposed instance.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
