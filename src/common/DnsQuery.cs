//------------------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">sayanch</owner>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;

    internal static class DnsQueryMethods
    {
        internal static int DNS_MAX_NAME_BUFFER_LENGTH = 256;

        internal static string GetDnsDomainName()
        {
            int domainNameLength = DNS_MAX_NAME_BUFFER_LENGTH * 2;
            IntPtr nameBuffer = Marshal.AllocCoTaskMem(domainNameLength);
            if (nameBuffer == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }
            int bufferLength = domainNameLength;
            int result = DnsQueryNativeMethods.DnsQueryConfig(DnsQueryNativeMethods.DNS_CONFIG_TYPE.DnsConfigPrimaryDomainName_W, 0, null, IntPtr.Zero, nameBuffer, out bufferLength);

            if (result == DnsQueryNativeMethods.ERROR_MORE_DATA)
            {
                //this means the allocated buffer was not long enough
                //We should free the old buffer, allocate a buffer as long as that specified by bufferLength
                Marshal.FreeCoTaskMem(nameBuffer);
                nameBuffer = Marshal.AllocCoTaskMem(bufferLength);
                if (nameBuffer == IntPtr.Zero)
                {
                    throw new OutOfMemoryException();
                }
                result = DnsQueryNativeMethods.DnsQueryConfig(DnsQueryNativeMethods.DNS_CONFIG_TYPE.DnsConfigPrimaryDomainName_W, 0, null, IntPtr.Zero, nameBuffer, out bufferLength);
            }

            if (result == 0)
            {
                string dnsDomainName = Marshal.PtrToStringUni(nameBuffer, bufferLength / 2);
                Marshal.FreeCoTaskMem(nameBuffer);

                return dnsDomainName;
            }

            Marshal.FreeCoTaskMem(nameBuffer);
            return null;
        }
    }

    internal static class DnsQueryNativeMethods
    {

        [DllImport("dnsapi", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern int DnsQueryConfig(
                  DNS_CONFIG_TYPE Config,
                  Int32 Flag,
                  string pwsAdapterName,
                  IntPtr pReserved,
                  IntPtr pBuffer,
                  out int BufferLength
        );

        internal enum DNS_CONFIG_TYPE
        {
            DnsConfigPrimaryDomainName_W,
            DnsConfigPrimaryDomainName_A,
            DnsConfigPrimaryDomainName_UTF8,
            DnsConfigAdapterDomainName_W,
            DnsConfigAdapterDomainName_A,
            DnsConfigAdapterDomainName_UTF8,
            DnsConfigDnsServerList,
            DnsConfigSearchList,
            DnsConfigAdapterInfo,
            DnsConfigPrimaryHostNameRegistrationEnabled,
            DnsConfigAdapterHostNameRegistrationEnabled,
            DnsConfigAddressRegistrationMaxCount,
            DnsConfigHostName_W,
            DnsConfigHostName_A,
            DnsConfigHostName_UTF8,
            DnsConfigFullHostName_W,
            DnsConfigFullHostName_A,
            DnsConfigFullHostName_UTF8
        };

        internal const int ERROR_MORE_DATA = 234;
    }
}
