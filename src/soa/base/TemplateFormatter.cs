// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.RuntimeTrace
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    /// An abstract class that the derives from to format ETW user payload
    /// into a string
    /// </summary>
    public abstract class TemplateFormatter
    {
        /// <summary>
        /// When overridden in a derived class, gets the message of the event
        /// </summary>
        protected abstract string Message { get; }

        /// <summary>
        /// Serialize the user payload into an object dic
        /// </summary>
        /// <param name="binaryData">indicating the user payload</param>
        /// <returns>returns the serialized object dic, keyed by names</returns>
        public Dictionary<string, object> ParseBinaryData(byte[] binaryData)
        {
            IntPtr payload = IntPtr.Zero;
            try
            {
                payload = Marshal.AllocHGlobal(binaryData.Length);
                Marshal.Copy(binaryData, 0, payload, binaryData.Length);
                return this.ParseBinaryDataInternal(payload);
            }
            finally
            {
                if (payload != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(payload);
                }
            }
        }

        /// <summary>
        /// Formats the user payload into a string
        /// </summary>
        /// <param name="binaryData">indicating the user payload</param>
        /// <returns>returns the formatted string</returns>
        public string Format(byte[] binaryData)
        {
            List<object> parameters = new List<object>();

            // Add one dummy parameters at first because
            // the given message is 1 based instead of 0
            // based.
            parameters.Add(String.Empty);
            parameters.AddRange(this.ParseBinaryData(binaryData).Values);
            return String.Format(this.Message, parameters.ToArray());
        }

        /// <summary>
        /// When overridden in a derived class, serialize the user payload
        /// into an object array
        /// </summary>
        /// <param name="pointer">
        /// indicating the pointer to the user payload
        /// </param>
        /// <returns>returns the serialized object dic, keyed by name</returns>
        protected abstract Dictionary<string, object> ParseBinaryDataInternal(IntPtr pointer);
    }
}
