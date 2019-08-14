namespace Microsoft.Hpc
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Text;

    public static class DomainUtil
    {
        public const int ErrorSuccess = 0;

        public static bool IsInDomain()
        {
            NetJoinStatus status = NetJoinStatus.NetSetupUnknownStatus;
            IntPtr pDomain = IntPtr.Zero;
            int result = NetGetJoinInformation(null, out pDomain, out status);
            if (pDomain != IntPtr.Zero)
            {
                NetApiBufferFree(pDomain);
            }
            if (result == ErrorSuccess)
            {
                return status == NetJoinStatus.NetSetupDomainName;
            }
            else
            {
                throw new SystemException("Failed to get domain info");
            }
        }

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int NetGetJoinInformation(string server, out IntPtr domain, out NetJoinStatus status);

        [DllImport("Netapi32.dll")]
        public static extern int NetApiBufferFree(IntPtr Buffer);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool LookupAccountName(
            string systemName,
            string accountName,
            byte[] sid,
            ref int sidLen,
            StringBuilder domainName,
            ref int domainNameLen,
            out SID_NAME_USE peUse);

        public static SecurityIdentifier LookupAccountName(
            string systemName,
            string accountName)
        {
            const int ERROR_INSUFFICIENT_BUFFER = 122;

            int lSidSize = 0;
            int lDomainNameSize = 0;
            SID_NAME_USE accountType;
            //First get the required buffer sizes for SID and domain name.
            LookupAccountName(systemName,
                              accountName,
                              null,
                              ref lSidSize,
                              null,
                              ref lDomainNameSize,
                              out accountType);

            if (Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
            {
                //Allocate the buffers with actual sizes that are required for SID and domain name.
                byte[] sid = new byte[lSidSize];
                var sbDomainName = new StringBuilder(lDomainNameSize);

                if (LookupAccountName(systemName,
                                      accountName,
                                      sid,
                                      ref lSidSize,
                                      sbDomainName,
                                      ref lDomainNameSize,
                                      out accountType))
                {
                    return new SecurityIdentifier(sid, 0);
                }
            }

            throw new Win32Exception();
        }

        public enum NetJoinStatus
        {
            NetSetupUnknownStatus = 0,
            NetSetupUnjoined,
            NetSetupWorkgroupName,
            NetSetupDomainName
        }

        public enum SID_NAME_USE
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer
        }
    }
}
