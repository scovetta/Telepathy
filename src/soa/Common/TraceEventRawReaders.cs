//-----------------------------------------------------------------------
// <copyright file="TraceEventRawReaders.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   Provide raw reader to read data from event
// </summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Raw reader to read data from event
    /// </summary>
    /// <remarks>
    /// This part of code is copied from ETWCore which is given by
    /// .Net team.
    /// TODO: Does this part of code works under Win 2012?
    /// </remarks>
    internal static class TraceEventRawReaders
    {
        unsafe internal static IntPtr Add(IntPtr pointer, int offset)
        {
            return (IntPtr)(((byte*)pointer) + offset);
        }

        unsafe internal static Guid ReadGuid(IntPtr pointer, int offset)
        {
            return *((Guid*)((byte*)pointer.ToPointer() + offset));
        }

        unsafe internal static double ReadDouble(IntPtr pointer, int offset)
        {
            return *((double*)((byte*)pointer.ToPointer() + offset));
        }

        unsafe internal static double ReadSingle(IntPtr pointer, int offset)
        {
            return *((float*)((byte*)pointer.ToPointer() + offset));
        }

        unsafe internal static long ReadInt64(IntPtr pointer, int offset)
        {
            return *((long*)((byte*)pointer.ToPointer() + offset));
        }

        unsafe internal static int ReadInt32(IntPtr pointer, int offset)
        {
            return *((int*)((byte*)pointer.ToPointer() + offset));
        }

        unsafe internal static short ReadInt16(IntPtr pointer, int offset)
        {
            return *((short*)((byte*)pointer.ToPointer() + offset));
        }

        unsafe internal static IntPtr ReadIntPtr(IntPtr pointer, int offset)
        {
            return *((IntPtr*)((byte*)pointer.ToPointer() + offset));
        }

        unsafe internal static byte ReadByte(IntPtr pointer, int offset)
        {
            return *((byte*)((byte*)pointer.ToPointer() + offset));
        }

        unsafe internal static bool ReadBoolean(IntPtr pointer, int offset)
        {
            return *((bool*)((byte*)pointer.ToPointer() + offset));
        }

        unsafe internal static string ReadUnicodeString(IntPtr pointer, int offset)
        {
            return Marshal.PtrToStringUni(new IntPtr((void*)((byte*)pointer.ToPointer() + offset)));
        }

        /// <summary>
        /// Assume that  'offset' bytes into the 'mofData' is a unicode 
        /// string.  Return the Offset after it is skipped.  This is intended
        /// to be used by subclasses trying to parse mofData 
        /// </summary>
        /// <param name="pointer">indicating the pointer</param>
        /// <param name="offset">the starting Offset</param>
        /// <returns>Offset just after the string</returns>
        internal static int SkipUnicodeString(IntPtr pointer, int offset)
        {
            IntPtr mofData = pointer;
            while (TraceEventRawReaders.ReadInt16(mofData, offset) != 0)
            {
                offset += 2;
            }

            offset += 2;
            return offset;
        }

        unsafe internal static string ReadAsciiString(IntPtr pointer, int offset)
        {
            string ret = Marshal.PtrToStringAnsi((IntPtr)(pointer.ToInt64() + offset));
            return ret;
        }
    }
}
