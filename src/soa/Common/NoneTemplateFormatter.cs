//------------------------------------------------------------------------------
// <copyright file="NoneTemplateFormatter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   An abstract class that the custom generated template formatter derives
//   from to format ETW user payload into a string
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides an implementation of TemplateFormatter which does not
    /// contains any custom data, it just return the given message
    /// </summary>
    public sealed class NoneTemplateFormatter : TemplateFormatter
    {
        /// <summary>
        /// Stores the message
        /// </summary>
        private string message;

        /// <summary>
        /// Gets the message of the event
        /// </summary>
        protected override string Message { get { return this.message; } }

        /// <summary>
        /// Initializes a new instance of the NoneTemplateFormatter class
        /// </summary>
        /// <param name="message">indicating the message</param>
        public NoneTemplateFormatter(string message)
        {
            this.message = message;
        }

        /// <summary>
        /// Serialize the user payload into an object array
        /// </summary>
        /// <param name="pointer">
        /// indicating the pointer to the user payload
        /// </param>
        /// <returns>returns the serialized object dic, keyed by name</returns>
        protected override Dictionary<string, object> ParseBinaryDataInternal(IntPtr pointer)
        {
            return new Dictionary<string, object>();
        }
    }
}
