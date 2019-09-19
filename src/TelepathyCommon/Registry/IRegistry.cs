// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace TelepathyCommon.Registry
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRegistry
    {
        Task DeleteValueAsync(string key, string name, CancellationToken token);

        /// <summary>
        ///     Get cluster registry properties
        /// </summary>
        /// <param name="propertyNames">the list of property names</param>
        /// <param name="token">the cancellation token</param>
        /// <returns>
        ///     The dictionary of property name and value pairs. For non-exist property, null string will be returned as the
        ///     value. If the input property names is null or empty, all properties will be returned.
        /// </returns>
        Task<IDictionary<string, string>> GetRegistryProperties(IList<string> propertyNames, CancellationToken token);

        /// <summary>
        ///     should return default (T) when not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<T> GetValueAsync<T>(string key, string name, CancellationToken token, T defaultValue = default);

        Task<object> GetValueAsync(string key, string name, CancellationToken token, object defaultValue = null);

        /// <summary>
        ///     Register a callback when the value identified by the key and name is created, changed or deleted.
        ///     The first time you register, you will get a value created event always if the value exists.
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
        Task MonitorRegistryKeyAsync<T>(string key, string name, TimeSpan checkPeriod, EventHandler<RegistryValueChangedArgs<T>> callback, CancellationToken token);

        /// <summary>
        ///     Set cluster registry properties
        /// </summary>
        /// <param name="properties">the dictionary of property name and value pairs.</param>
        /// <param name="token">the cancellation token</param>
        /// <returns></returns>
        Task SetRegistryProperties(IDictionary<string, object> properties, CancellationToken token);

        Task SetValueAsync<T>(string key, string name, T value, CancellationToken token);
    }
}