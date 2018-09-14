//-------------------------------------------------------------------------------------------------
// <copyright file="OSHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------
namespace Microsoft.Hpc.Utils
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Runtime.InteropServices;
    using System.ServiceProcess;

    public class OSHelper
    {
        private class DhcpClient : IDisposable
        {
            /// <summary>
            /// name of the VM bus device
            /// </summary>
            public const string VMBusName = "vmbus";

            /// <summary>
            /// DHCP option that is specific to Windows Azure
            /// </summary>
            public const uint AzureSpecificDhcpOption = 245;

            /// <summary>
            /// Length of DHCP parameter in byte
            /// </summary>
            public const int DhcpParamLength = 4;

            public const byte DHCPCAPI_REQUEST_SYNCHRONOUS = 0x02;

            /// <summary>
            /// The flag that indicates there is not 
            /// enough buffer space.
            /// </summary>
            private const uint ERROR_MORE_DATA = 124;

            /// <summary>
            /// Maximum buffer size.
            /// </summary>
            private const uint maxBufferSize = 102400;

            /// <summary>
            /// Initial buffer size.
            /// </summary>
            private const uint initBufferSize = 5120;

            private bool disposed = false;

            /// <summary>
            /// Creates a default instance of DhcpClient
            /// </summary>
            public DhcpClient()
            {
                uint version;
                int err = DhcpCApiInitialize(out version);

                if (err != 0)
                {
                    throw new Win32Exception(err);
                }
            }

            [Flags]
            internal enum DhcpRequestFlags : uint
            {
                DhcpCApi_Request_Persistent = 0x01,
                DhcpCApi_Request_Synchronous = 0x02,
                DhcpCApi_Request_Asynchronous = 0x04,
                DhcpCApi_Request_Cancel = 0x08,
                DhcpCApi_Request_Mask = 0x0F
            }

            /// <summary>
            /// Requests DHCP parameter data.
            /// </summary>
            /// <param name="adapterName">Name of the NIC adaptor</param>
            /// <param name="optionId">The option to obtain</param>
            /// <returns>The DHCP paramter requested</returns>
            public byte[] DhcpRequestParams(string adapterName, uint optionId)
            {
                uint bufferSize = initBufferSize;

                while (bufferSize < maxBufferSize)
                {
                    IntPtr buffer = IntPtr.Zero;
                    IntPtr recdParamsPtr = IntPtr.Zero;

                    try
                    {
                        buffer = Marshal.AllocHGlobal((int)bufferSize);

                        DhcpCApi_Params_Array sendParams = new DhcpCApi_Params_Array();
                        sendParams.nParams = 0;
                        sendParams.Params = IntPtr.Zero;

                        DhcpCApi_Params recv = new DhcpCApi_Params
                        {
                            Flags = 0x0,
                            OptionId = optionId,
                            IsVendor = false,
                            Data = IntPtr.Zero,
                            nBytesData = 0
                        };

                        recdParamsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(recv));
                        Marshal.StructureToPtr(recv, recdParamsPtr, false);

                        DhcpCApi_Params_Array recdParams = new DhcpCApi_Params_Array();
                        recdParams.nParams = 1;
                        recdParams.Params = recdParamsPtr;

                        DhcpRequestFlags flags = DhcpRequestFlags.DhcpCApi_Request_Synchronous;

                        int err = DhcpRequestParams(
                            flags,
                            IntPtr.Zero,
                            adapterName,
                            IntPtr.Zero,
                            sendParams,
                            recdParams,
                            buffer,
                            ref bufferSize,
                            null);

                        if (err == ERROR_MORE_DATA)
                        {
                            bufferSize *= 2;
                            continue;
                        }

                        if (err != 0)
                        {
                            throw new Win32Exception(err);
                        }

                        recv = (DhcpCApi_Params)
                            Marshal.PtrToStructure(recdParamsPtr, typeof(DhcpCApi_Params));

                        if (recv.Data == IntPtr.Zero)
                        {
                            return null;
                        }

                        byte[] data = new byte[recv.nBytesData];
                        Marshal.Copy(recv.Data, data, 0, (int)recv.nBytesData);

                        return data;
                    }
                    finally
                    {
                        if (buffer != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(buffer);
                        }

                        if (recdParamsPtr != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(recdParamsPtr);
                        }
                    }
                }

                throw new InvalidOperationException(
                    "The operation has exceeded the maximum buffer size allowed.");
            }

            #region Dispose

            /// <summary>
            /// Call the native DhcpCApiCleanup method
            /// </summary>
            public void Dispose()
            {
                // Dispose of unmanaged resources.
                this.Dispose(true);
                // Suppress finalization.
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (this.disposed)
                    return;

                try
                {
                    DhcpCApiCleanup();
                }
                catch
                {
                }

                this.disposed = true;
            }

            #endregion

            #region P/Invoke

            [DllImport("dhcpcsvc.dll",
                EntryPoint = "DhcpRequestParams",
                CharSet = CharSet.Unicode,
                SetLastError = false)]
            internal static extern int DhcpRequestParams(
                DhcpRequestFlags Flags,
                IntPtr Reserved,
                string AdapterName,
                IntPtr ClassId,
                DhcpCApi_Params_Array SendParams,
                DhcpCApi_Params_Array RecdParams,
                IntPtr Buffer,
                ref UInt32 pSize,
                string RequestIdStr);

            [DllImport("dhcpcsvc.dll",
                EntryPoint = "DhcpUndoRequestParams",
                CharSet = CharSet.Unicode,
                SetLastError = false)]
            internal static extern int DhcpUndoRequestParams(
                uint Flags,
                IntPtr Reserved,
                string AdapterName,
                string RequestIdStr);

            [DllImport("dhcpcsvc.dll",
                EntryPoint = "DhcpCApiInitialize",
                CharSet = CharSet.Unicode,
                SetLastError = false)]
            internal static extern int DhcpCApiInitialize(out uint Version);

            [DllImport("dhcpcsvc.dll",
                EntryPoint = "DhcpCApiCleanup",
                CharSet = CharSet.Unicode,
                SetLastError = false)]
            internal static extern int DhcpCApiCleanup();

            #endregion

            /// <summary>
            /// Gets the network interfaces that are enabled for DHCP. 
            /// </summary>
            /// <returns>List of DHCP enabled network interfaces.</returns>
            internal static IEnumerable<NetworkInterface> GetDhcpInterfaces()
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet ||
                        !nic.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        continue;
                    }

                    IPInterfaceProperties props = nic.GetIPProperties();

                    if (props == null)
                    {
                        continue;
                    }

                    IPv4InterfaceProperties v4props = props.GetIPv4Properties();

                    if (v4props == null || !v4props.IsDhcpEnabled)
                    {
                        continue;
                    }

                    yield return nic;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct DhcpCApi_Params_Array
            {
                public UInt32 nParams;
                public IntPtr Params;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct DhcpCApi_Params
            {
                public UInt32 Flags;
                public UInt32 OptionId;
                [MarshalAs(UnmanagedType.Bool)]
                public bool IsVendor;
                public IntPtr Data;
                public UInt32 nBytesData;
            }
        }

        /// <summary>
        /// Check whether the server is VM on Azure IaaS
        /// </summary>
        /// <remarks>
        /// The method checks if the computer is a Hyper-V guest by verifying that the 
        /// vmbus driver is existing. If yes, the script does a 
        /// platform invoke to call Win32 function DhcpRequestParams to query the DHCP 
        /// server for option 245. Since Azure VMs must be configured for dynamic IP 
        /// addresses, and option 245 is specific to Windows Azure, 
        /// this confirms the VM is running in Windows Azure.
        /// </remarks>
        public static bool IsOnAzureIaaSVM()
        {
            bool onIaas = false;

            var vmBus = ServiceController.GetDevices().FirstOrDefault(
                d => d.ServiceName.Equals(DhcpClient.VMBusName, StringComparison.InvariantCultureIgnoreCase));

            if (vmBus != null && vmBus.Status == ServiceControllerStatus.Running)
            {
                try
                {
                    using (var client = new DhcpClient())
                    {
                        foreach (var dhcp in DhcpClient.GetDhcpInterfaces())
                        {
                            var val = client.DhcpRequestParams(dhcp.Id, DhcpClient.AzureSpecificDhcpOption);
                            if (val != null && val.Length == DhcpClient.DhcpParamLength)
                            {
                                onIaas = true;
                                break;
                            }
                        }
                    }
                }
                catch (Win32Exception e)
                {
                    System.Diagnostics.Trace.TraceError("Exception occurs during verify whether the VM is Azure IaaS VM, {0}", e);
                }
            }

            return onIaas;
        }

        [DllImport("Win8ProcWrapper.dll")]
        static extern int GetTotalActiveProcessorCount();

        /// <summary>
        /// If the system has more than 64 cores, they will be put to several processor groups.
        /// The Environment.ProcessorCount can only gets the number of installed cores in group 0.
        /// If the OS is Win8 or later, use the new API to get the total core count so that it 
        /// can consider all the processor groups.
        /// Win7 also has this API. However, in Win7 we are unable to utilize the cores in 
        /// other groups. So it's better to ignore the other cores in Win7.
        /// </summary>
        /// <returns></returns>
        public static int GetTotalCoreCount()
        {
            if (IsWin8OrLater.Value)
            {
                return GetTotalActiveProcessorCount();
            }
            else
            {
                return Environment.ProcessorCount;
            }
        }

        static Lazy<bool> IsWin8OrLater = new Lazy<bool>(() => Environment.OSVersion.Version >= new Version(6, 2));
    }
}
