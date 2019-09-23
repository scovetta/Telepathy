// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common.Registry
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class RegistryProperty : IRegistry
    {
        public abstract Task DeleteValueAsync(string key, string name, CancellationToken token);

        public async Task<IDictionary<string, string>> GetRegistryProperties(IList<string> propertyNames, CancellationToken token)
        {
            throw new NotSupportedException();
        }

        public abstract Task<T> GetValueAsync<T>(string key, string name, CancellationToken token, T defaultValue = default);

        public virtual async Task<object> GetValueAsync(string key, string name, CancellationToken token, object defaultValue = null)
        {
            return await this.GetValueAsync<object>(key, name, token, defaultValue).ConfigureAwait(false);
        }

        public abstract Task MonitorRegistryKeyAsync<T>(string key, string name, TimeSpan checkPeriod, EventHandler<RegistryValueChangedArgs<T>> callback, CancellationToken token);

        public async Task SetRegistryProperties(IDictionary<string, object> properties, CancellationToken token)
        {
            throw new NotSupportedException();
        }

        public abstract Task SetValueAsync<T>(string key, string name, T value, CancellationToken token);
    }
}