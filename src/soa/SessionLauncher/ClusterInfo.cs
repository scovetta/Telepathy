// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher
{
    using Microsoft.Telepathy.Session.Interface;

    internal class ClusterInfo
    {
        public ClusterInfoContract Contract { get; private set; }

        public ClusterInfo()
        {
            this.Contract = new ClusterInfoContract();
        } 

#if HPCPACK
        public ClusterInfo()
        {
            Contract = new ClusterInfoContract();
            Contract.ClusterName = HpcContext.Get().GetClusterNameAsync().GetAwaiter().GetResult();
            Guid id = HpcContext.Get().GetClusterIdAsync().GetAwaiter().GetResult();
            Contract.ClusterId = id == Guid.Empty ? null : id.ToString().ToLowerInvariant();
            Contract.NetworkTopology = HpcContext.Get().Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.NetworkTopology, HpcContext.Get().CancellationToken).GetAwaiter().GetResult();
            Contract.AzureStorageConnectionString = HpcContext.Get().Registry.GetValueAsync<string>(HpcConstants.HpcSecurityRegKey, HpcConstants.AzureStorageConnectionString, HpcContext.Get().CancellationToken).GetAwaiter().GetResult();
            Monitor();
        }

        public event EventHandler OnAzureStorageConnectionStringOrClusterIdUpdated;

        private void Monitor()
        {
            HpcContext.Get().Registry.MonitorRegistryKeyAsync<string>(
                HpcConstants.HpcFullKeyName,
                HpcConstants.NetworkTopology,
                TimeSpan.FromSeconds(30),
                (o, e) => {
                    if (e.ValueChangeType == RegistryValueChangedArgs<string>.ChangeType.Created || e.ValueChangeType == RegistryValueChangedArgs<string>.ChangeType.Modified)
                    {
                        this.Contract.NetworkTopology = e.NewValue;
                        TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] ClusterInfo Registry NetworkTopology is created or updated: {0}", e.NewValue);
                    }
                },
                HpcContext.Get().CancellationToken);

            HpcContext.Get().Registry.MonitorRegistryKeyAsync<string>(
                HpcConstants.HpcSecurityRegKey,
                HpcConstants.AzureStorageConnectionString,
                TimeSpan.FromSeconds(30),
                (o, e) => {
                    if (e.ValueChangeType == RegistryValueChangedArgs<string>.ChangeType.Created || e.ValueChangeType == RegistryValueChangedArgs<string>.ChangeType.Modified)
                    {
                        this.Contract.AzureStorageConnectionString = e.NewValue;
                        TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] ClusterInfo Registry AzureStorageConnectionString is created or updated: ***");
                        OnAzureStorageConnectionStringOrClusterIdUpdated?.Invoke(o, e);
                    }
                },
                HpcContext.Get().CancellationToken);

            HpcContext.Get().Registry.MonitorRegistryKeyAsync<Guid>(
                HpcConstants.HpcFullKeyName,
                HpcConstants.ClusterIdRegVal,
                TimeSpan.FromSeconds(30),
                (o, e) => {
                    if (e.ValueChangeType == RegistryValueChangedArgs<Guid>.ChangeType.Created || e.ValueChangeType == RegistryValueChangedArgs<Guid>.ChangeType.Modified)
                    {
                        this.Contract.ClusterId = e.NewValue.ToString().ToLowerInvariant();
                        TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] ClusterInfo Registry ClusterId is created or updated: {0}", e.NewValue);
                        OnAzureStorageConnectionStringOrClusterIdUpdated?.Invoke(o, e);
                    }
                },
                HpcContext.Get().CancellationToken);
        }
#endif
    }
}
