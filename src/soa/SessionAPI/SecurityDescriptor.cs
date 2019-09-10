namespace Microsoft.Hpc
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    using Microsoft.Win32.SafeHandles;

    public sealed class SecurityDescriptor : SafeHandleZeroOrMinusOneIsInvalid
    {
        const int SDDL_REVISION_1 = 1;

        private SecurityDescriptor()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            SecurityDescriptorNativeMethods.LocalFree(this.handle);
            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared source file")]
        internal static SecurityDescriptor FromSddl(string sddl)
        {
            SecurityDescriptor desc;
            int size;

            if (!SecurityDescriptorNativeMethods.ConvertStringSecurityDescriptorToSecurityDescriptor(sddl, SDDL_REVISION_1, out desc, out size))
            {
                throw new Win32Exception();
            }

            return desc;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared source file")]
        internal bool CheckAccess(IntPtr clientToken, int desiredAccess, SecurityDescriptorNativeMethods.GENERIC_MAPPING genericMapping)
        {
            SecurityDescriptorNativeMethods.PRIVILEGE_SET privilege = new SecurityDescriptorNativeMethods.PRIVILEGE_SET();
            int privilegeSize = SecurityDescriptorNativeMethods.PRIVILEGE_SET.Size;
            int grantedAccess;
            bool result;
            if (!SecurityDescriptorNativeMethods.AccessCheck(this, clientToken, desiredAccess, genericMapping, privilege, ref privilegeSize, out grantedAccess, out result))
            {
                throw new Win32Exception();
            }

            return result;
        }        
    }

    static class SecurityDescriptorNativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Shared source file")]
        internal class GENERIC_MAPPING
        {
            internal GENERIC_MAPPING(int genericRead, int genericWrite, int genericExecute, int genericAll)
            {
                this.GenericRead = genericRead;
                this.GenericWrite = genericWrite;
                this.GenericExecute = genericExecute;
                this.GenericAll = genericAll;
            }

            public int GenericRead;
            public int GenericWrite;
            public int GenericExecute;
            public int GenericAll;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LUID
        {
            internal const int Size = 4 + 8;

            int LowPart;
            long HighPart;            
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LUID_AND_ATTRIBUTES
        {
            internal const int Size = LUID.Size + 4;

            LUID Luid;
            int Attributes;            
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class PRIVILEGE_SET
        {
            //at this moment we only support 1 privilege in the set
            internal const int Size = 4 + 4 + LUID_AND_ATTRIBUTES.Size;

            int PrivilegeCount; //=0
            int Control;
            //at this moment we only support 1 privilege in the set
            LUID_AND_ATTRIBUTES Privilege;
        }

        [DllImport("Advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared source file")]
        internal static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
            string Sddl,
            int sddlVersion,
            out SecurityDescriptor securityDescriptor,
            out int descSize);

        [DllImport("Advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared source file")]
        internal static extern bool AccessCheck(
            SecurityDescriptor securityDescriptor,
            IntPtr clientToken,
            int desiredAccess,
            GENERIC_MAPPING genericMapping,
            PRIVILEGE_SET privilegeSet,
            ref int privilegeSetLength,
            out int grandtedAccess,
            [MarshalAs(UnmanagedType.Bool)] out bool accessStatus);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LocalFree(IntPtr mem);
    }
}