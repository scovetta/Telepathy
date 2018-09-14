//-----------------------------------------------------------------------
// <copyright file="ConcurrentStreamablePackageException.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//    Provides an exception representing exceptions details sending back
//    through the concurrent streamable package
// </summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Provides an exception representing exceptions details sending back
    /// through the concurrent streamable package
    /// </summary>
    /// <remarks>
    /// The exception details contains all exception happens when dumping
    /// traces from different nodes (both azure nodes and on premise nodes).
    /// </remarks>
    internal sealed class ConcurrentStreamablePackageException : Exception
    {
        /// <summary>
        /// Stores the exception message
        /// </summary>
        private string message;

        /// <summary>
        /// Initializes a new instance of the ConcurrentStreamablePackageException class
        /// </summary>
        /// <param name="reader">
        /// indicating the binary reader instance which exception details
        /// could be read from
        /// </param>
        public ConcurrentStreamablePackageException(BinaryReader reader)
        {
            this.ExceptionDetails = new Dictionary<string, string[]>();
            StringBuilder sb = new StringBuilder();
            try
            {
                while (true)
                {
                    string node = reader.ReadString();
                    int count = reader.ReadInt32();
                    Debug.Assert(count > 0, "Count must be greater than 0.");
                    string[] exceptionDetails = new string[count];
                    sb.AppendLine(node + ":");
                    for (int i = 0; i < count; i++)
                    {
                        exceptionDetails[i] = reader.ReadString();
                        sb.AppendLine(exceptionDetails[i]);
                    }

                    // There won't be two sections with the same node name
                    this.ExceptionDetails.Add(node, exceptionDetails);
                }
            }
            catch (EndOfStreamException)
            {
            }

            this.message = sb.ToString();
        }

        /// <summary>
        /// Gets the exception message
        /// </summary>
        public override string Message
        {
            get
            {
                return this.message;
            }
        }

        /// <summary>
        /// Gets the exception details keyed by the nodes
        /// </summary>
        public Dictionary<string, string[]> ExceptionDetails
        {
            get;
            private set;
        }
    }
}
