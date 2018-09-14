//------------------------------------------------------------------------------
// <copyright file="CredentialHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Access window credential set to persist/fetech user's cred
// </summary>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;

namespace Microsoft.Hpc
{
    internal static class CredentialHelper
    {
        private enum CRED_TYPE : uint
        {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            GENERIC_CERTIFICATE = 5,
            DOMAIN_EXTENDED = 6,
            MAXIMUM = 7,
            MAXIMUM_EX = (MAXIMUM + 1000),
        }


        private enum CRED_PERSIST : uint
        {
            SESSION = 1,
            LOCAL_MACHINE = 2,
            ENTERPRISE = 3,
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NativeCredential
        {
            public UInt32 Flags;
            public CRED_TYPE Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public UInt32 CredentialBlobSize;
            public IntPtr CredentialBlob;
            public UInt32 Persist;
            public UInt32 AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;
        }

        [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredEnumerate(string filter, int flag, out int count, out IntPtr pCredentials);


        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag, out IntPtr CredentialPtr);


        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredWrite([In] ref NativeCredential userCredential, [In] UInt32 flags);


        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CredDelete(string target, int type, int flags);


        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern void CredFree([In] IntPtr cred);


        // the key is <HeadNodeName>\<UserName>
        private static string keyFormat = @"{0}\{1}";


        /// <summary>
        /// Use headNodeName\userName as a key to save the password.
        /// </summary>
        internal static void PersistPassword(string headNodeName, string userName, byte[] encryptedPassword)
        {
            WriteCred(string.Format(keyFormat, headNodeName, userName), encryptedPassword);
        }


        /// <summary>
        /// Get password from the credential set by key headNodeName\userName.
        /// </summary>
        internal static byte[] FetchPassword(string headNodeName, string userName)
        {
            //TODO: SF: headnodeName is a gateway string
            return ReadCred(string.Format(keyFormat, headNodeName, userName));
        }


        /// <summary>
        /// Remove the password from the credential set.
        /// </summary>
        internal static void PurgePassword(string headNodeName, string userName)
        {
            if (!CredDelete(string.Format(keyFormat, headNodeName, userName), (int)CRED_TYPE.GENERIC, 0))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Query the credential cache, get the first one and return username.
        /// </summary>
        /// <returns>user name</returns>
        internal static string FetchDefaultUsername(string headNodeName)
        {
            //TODO: SF: headnodeName is a gateway string
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int count;
                string filter = string.Format(@"{0}\*", headNodeName);
                if (CredentialHelper.CredEnumerate(filter, 0, out count, out ptr))
                {
                    NativeCredential c = (NativeCredential)Marshal.PtrToStructure(Marshal.ReadIntPtr(ptr), typeof(NativeCredential));
                    string target = Marshal.PtrToStringUni(c.TargetName);
                    string[] tmp = target.Split(new char[] { '\\' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    return tmp[1];
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    CredFree(ptr);
                }
            }

            return null;
        }

        /// <summary>
        /// Write credential to the win cedential set
        /// </summary>
        private static void WriteCred(string key, byte[] byteArray)
        {
            NativeCredential ncred = new NativeCredential();
            GCHandle handle = GCHandle.Alloc(byteArray, GCHandleType.Pinned);

            try
            {
                ncred.TargetName = Marshal.StringToCoTaskMemUni(key);
                ncred.UserName = Marshal.StringToCoTaskMemUni(WindowsIdentity.GetCurrent().Name);

                ncred.CredentialBlobSize = (uint)byteArray.Length;
                ncred.CredentialBlob = Marshal.UnsafeAddrOfPinnedArrayElement(byteArray, 0);

                ncred.Persist = (uint)CRED_PERSIST.LOCAL_MACHINE;
                ncred.Type = CRED_TYPE.GENERIC;

                ncred.TargetAlias = IntPtr.Zero;
                ncred.AttributeCount = 0;
                ncred.Attributes = IntPtr.Zero;
                ncred.Comment = IntPtr.Zero;

                if (!CredWrite(ref ncred, 0))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (ncred.UserName != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ncred.UserName);
                }

                if (ncred.TargetName != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ncred.TargetName);
                }

                handle.Free();
            }
        }


        /// <summary>
        /// Read credential from the win cedential set
        /// return null if it doesn't exist
        /// </summary>
        private static byte[] ReadCred(string key)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                if (CredRead(key, CRED_TYPE.GENERIC, 0, out ptr))
                {
                    IntPtr offset = Marshal.OffsetOf(typeof(NativeCredential), "CredentialBlobSize");
                    int size = Marshal.ReadInt32(new IntPtr(ptr.ToInt64() + offset.ToInt64()));

                    byte[] cred = new byte[size];
                    offset = Marshal.OffsetOf(typeof(NativeCredential), "CredentialBlob");
                    IntPtr data = Marshal.ReadIntPtr(new IntPtr(ptr.ToInt64() + offset.ToInt64()));
                    Marshal.Copy(data, cred, 0, size);
                    return cred;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    CredFree(ptr);
                }
            }
        }

        /// <summary>
        /// Find the credential for the server name and he username and password in the credential manager
        /// The username and password are null if they cannot be found
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        internal static void ReadUnFormattedCred(string serverName, out string userName, out SecureString securePassword)
        {
            IntPtr ptr = IntPtr.Zero;
            userName = null;
            securePassword = null;
            try
            {
                if (CredRead(serverName, CRED_TYPE.GENERIC, 0, out ptr))
                {
                    IntPtr offset = Marshal.OffsetOf(typeof(NativeCredential), "UserName");
                    IntPtr usernameData = Marshal.ReadIntPtr(new IntPtr(ptr.ToInt64() + offset.ToInt64()));
                    userName = Marshal.PtrToStringUni(usernameData);

                    offset = Marshal.OffsetOf(typeof(NativeCredential), "CredentialBlobSize");
                    int size = Marshal.ReadInt32(new IntPtr(ptr.ToInt64() + offset.ToInt64()));


                    offset = Marshal.OffsetOf(typeof(NativeCredential), "CredentialBlob");
                    IntPtr passwordData = Marshal.ReadIntPtr(new IntPtr(ptr.ToInt64() + offset.ToInt64()));

                    unsafe
                    {
                        securePassword = new SecureString((char*)passwordData.ToPointer(), size / 2);
                    }
                }

            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    CredFree(ptr);
                }
            }
        }

        internal static void WriteUnformattedCred(string serverName, string userName, SecureString password)
        {
            NativeCredential ncred = new NativeCredential();

            try
            {
                ncred.TargetName = Marshal.StringToCoTaskMemUni(serverName);
                ncred.UserName = Marshal.StringToCoTaskMemUni(userName);


                ncred.CredentialBlobSize =(uint) password.Length * 2;
                ncred.CredentialBlob = Marshal.SecureStringToCoTaskMemUnicode(password);

                ncred.Persist = (uint)CRED_PERSIST.LOCAL_MACHINE;
                ncred.Type = CRED_TYPE.GENERIC;

                ncred.TargetAlias = IntPtr.Zero;
                ncred.AttributeCount = 0;
                ncred.Attributes = IntPtr.Zero;
                ncred.Comment = IntPtr.Zero;

                if (!CredWrite(ref ncred, 0))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (ncred.UserName != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ncred.UserName);
                }

                if (ncred.TargetName != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ncred.TargetName);
                }

                if (ncred.CredentialBlob != IntPtr.Zero)
                {
                    Marshal.ZeroFreeCoTaskMemUnicode(ncred.CredentialBlob);
                }
            }

        }        

    }
}
