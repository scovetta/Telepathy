using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelepathyCommon.HpcContext;

namespace TelepathyCommon.Registry
{
    public abstract class RegistryProperty : IRegistry
    {
        public abstract Task DeleteValueAsync(string key, string name, CancellationToken token);

        public abstract Task<T> GetValueAsync<T>(string key, string name, CancellationToken token, T defaultValue = default(T));

        public virtual async Task<object> GetValueAsync(string key, string name, CancellationToken token, object defaultValue = null)
        {
            return await this.GetValueAsync<object>(key, name, token, defaultValue).ConfigureAwait(false);
        }

        public abstract Task MonitorRegistryKeyAsync<T>(string key, string name, TimeSpan checkPeriod, EventHandler<RegistryValueChangedArgs<T>> callback, CancellationToken token);

        public abstract Task SetValueAsync<T>(string key, string name, T value, CancellationToken token);

        public async Task<IDictionary<string, string>> GetRegistryProperties(IList<string> propertyNames, CancellationToken token)
        {
            IDictionary<string, string> properties = new Dictionary<string, string>();
            
            if (propertyNames != null && propertyNames.Count > 0)
            {
                foreach (var propertyName in propertyNames)
                {
                    if (propertyName != null && HpcConstants.ReliableProperties.ContainsKey(propertyName))
                    {
                        var reliableProp = HpcConstants.ReliableProperties[propertyName];
                        object propertyValue = await this.GetValueAsync(reliableProp.ParentName, propertyName, TelepathyContext.Get().CancellationToken, null).ConfigureAwait(false);
                        properties.Add(propertyName, this.GetStringValue(propertyValue));
                    }
                }
                return properties;
            }

            // return all properties
            foreach (var rp in HpcConstants.ReliableProperties)
            {
                object propertyValue = await GetValueAsync(rp.Value.ParentName, rp.Key, TelepathyContext.Get().CancellationToken, null).ConfigureAwait(false);
                properties.Add(rp.Key, propertyValue?.ToString());
            }

            return properties;
        }

        private string GetStringValue(object obj)
        {
            string value = null;
            if (obj == null)
            {
                return value;
            }

            Type t = obj.GetType();
            if (t == typeof(string[]))
            {
                value = string.Join(",", (string[])obj);
            }
            else if (t == typeof(byte[]))
            {
                value = Convert.ToBase64String((byte[])obj);
            }
            else
            {
                value = obj.ToString();
            }

            return value;
        }

        public async Task SetRegistryProperties(IDictionary<string, object> properties, CancellationToken token)
        {
            StringBuilder exceptionMessages = new StringBuilder();
            foreach (KeyValuePair<string, object> kv in properties)
            {
                var propertyType = kv.Value.GetType();
                var propertyValue = kv.Value;
                var propertyName = kv.Key;
                if (!HpcConstants.ReliableProperties.ContainsKey(kv.Key))
                {
                    exceptionMessages.AppendLine($"Unknown property name {propertyName}.");
                    continue;
                }

                var reliableProp = HpcConstants.ReliableProperties[propertyName];
                if (reliableProp.ReadOnly)
                {
                    exceptionMessages.AppendLine($"Property {propertyName} is not allowed to modify.");
                    continue;
                }

                if (propertyType != reliableProp.ValueType)
                {
                    exceptionMessages.AppendLine($"Type of property {propertyName} should be {reliableProp.ValueType}.");
                    continue;
                }

                IRegistry registry = TelepathyContext.Get().Registry;
                MethodInfo method = typeof(IRegistry).GetMethod("SetValueAsync");
                await ((Task)method.MakeGenericMethod(propertyType).Invoke(registry, new[] { reliableProp.ParentName, propertyName, propertyValue, TelepathyContext.Get().CancellationToken })).ConfigureAwait(false);
            }

            if (exceptionMessages.Length > 0)
            {
                throw new ArgumentException($"One or more properties were not set correctly: {exceptionMessages}");
            }
        }
    }
}
