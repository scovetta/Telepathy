namespace TelepathyCommon.Registry
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Win32;

    public abstract class WindowsRegistryBase : RegistryProperty, IDisposable
    {
        private static readonly Task CompletedTask =
#if net40
            TaskEx.FromResult(0);
#else
            Task.FromResult(0);
#endif

        private readonly List<IDisposable> watchers = new List<IDisposable>();

        private bool disposedValue; // To detect redundant calls

        public override Task DeleteValueAsync(string key, string name, CancellationToken token)
        {
            using (var regKey = this.CreateOrOpenSubKey(key))
            {
                if (regKey == null)
                {
                    // error handling
                    throw new InvalidOperationException(string.Format("The registry key {0} under {1} is null", name, key));
                }

                regKey.DeleteValue(name);
            }

            return CompletedTask;
        }

        public void Dispose()
        {
            this.Dispose(true);

            // Suppress finalization of this disposed instance.
            GC.SuppressFinalize(this);
        }

        public RegistryKey GetRootKey(string rootKeyName)
        {
            switch (rootKeyName)
            {
                case "HKEY_CURRENT_USER":
                    return Registry.CurrentUser;

                case "HKEY_LOCAL_MACHINE":
                    return Registry.LocalMachine;

                case "HKEY_CLASSES_ROOT":
                    return Registry.ClassesRoot;

                case "HKEY_USERS":
                    return Registry.Users;

                case "HKEY_PERFORMANCE_DATA":
                    return Registry.PerformanceData;

                case "HKEY_CURRENT_CONFIG":
                    return Registry.CurrentConfig;

                case "HKEY_DYN_DATA":
                    return Registry.CurrentConfig;
            }

            return null;
        }

        public override Task<T> GetValueAsync<T>(string key, string name, CancellationToken token, T defaultValue = default)
        {
            T res;
            if (typeof(T) == typeof(Guid))
            {
                var guidStr = (string)Registry.GetValue(key, name, string.Empty);
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
                var value = Registry.GetValue(key, name, defaultValue);
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

        /// <summary>
        ///     Register a callback when the value identified by the key and name is created, changed or deleted.
        ///     The first time you register, you will get a value created event always.
        /// </summary>
        /// <typeparam name="T">The value type</typeparam>
        /// <param name="key">the key</param>
        /// <param name="name">the name.</param>
        /// <param name="checkPeriod">the check period.</param>
        /// <param name="callback">the callback.</param>
        /// <param name="token">cancel this token to cancel the registration.</param>
        /// <returns>
        ///     The task which is running during the whole monitoring process. Exceptions happened during this process is
        ///     carried back by the task.
        /// </returns>
        public override Task MonitorRegistryKeyAsync<T>(string key, string name, TimeSpan checkPeriod, EventHandler<RegistryValueChangedArgs<T>> callback, CancellationToken token)
        {
            var watcher = new WindowsRegistryWatcher<T>(key, name);
            watcher.InstanceUpdated += callback;
            this.watchers.Add(watcher);
            return CompletedTask;
        }

        public override Task SetValueAsync<T>(string key, string name, T value, CancellationToken token)
        {
            using (var regKey = this.CreateOrOpenSubKey(key))
            {
                if (regKey == null)
                {
                    // error handling
                    throw new InvalidOperationException(string.Format("The registry key {0} under {1} is null", name, key));
                }

                regKey.SetValue(name, value);
            }

            return CompletedTask;
        }

        protected abstract RegistryKey CreateOrOpenSubKey(string key);

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    foreach (var watcher in this.watchers)
                    {
                        watcher.Dispose();
                    }
                }

                this.disposedValue = true;
            }
        }
    }
}