// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Data
{

    // Important!: this class is only intended to be used in session API and subjected to change
    // TODO: Trim the enum down

    public enum NodeLocation
    {
        /// <summary>
        ///   <para>The node is not hosted in Windows Azure. This enumeration member represents a value of 1.</para>
        /// </summary>
        OnPremise = 0x1,
        /// <summary>
        ///   <para>The node is Windows Azure worker node. This enumeration member represents a value of 2.</para>
        /// </summary>
        Azure = 0x2,
        /// <summary>
        ///   <para>The node is Windows Azure virtual machine worker node. This enumeration member represents a value of 3.</para>
        /// </summary>
        AzureVM = 0x3,
        /// <summary>
        ///   <para>The node is Windows Azure node manager that is in the 
        /// same deployment with the Windows Azure scheduler. This enumeration member represents a value of 4.</para>
        /// </summary>
        AzureEmbedded = 0x4,
        /// <summary>
        ///   <para>The node is Windows Azure virtual machine node manager that is in 
        /// the same deployment with the Windows Azure scheduler. This enumeration member represents a value of 5.</para>
        /// </summary>
        AzureEmbeddedVM = 0x5,
        /// <summary>
        ///   <para>The node is unmanaged worker node. This enumeration member represents a value of 6.</para>
        /// </summary>
        UnmanagedResource = 0x6,
        /// <summary>
        ///   <para>The node is Linux worker node. This enumeration member represents a value of 7.</para>
        /// </summary>
        Linux = 0x7,
        /// <summary>
        ///   <para>The node is Azure Batch worker node. This enumeration member represents a value of 8.</para>
        /// </summary>
        AzureBatch = 0x8,
        /// <summary>
        ///   <para>The node is not domain joined worker node. This enumeration member represents a value of 9.</para>
        /// </summary>
        NonDomainJoined = 0x9,
    }
}
