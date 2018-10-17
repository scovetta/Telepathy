//------------------------------------------------------------------------------
// <copyright file="SessionInfoContract.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The information about a cluster (in service fabric)
// </summary>
//------------------------------------------------------------------------------

using Microsoft.Hpc.RuntimeTrace;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher
{
    internal class ClusterInfo
    {
        public ClusterInfoContract Contract { get; private set; }

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
    }
}
