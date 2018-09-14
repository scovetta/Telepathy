//-----------------------------------------------------------------------
// <copyright file="TraceEventBlockHeader.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   Provides the header of the trace event block.
// </summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics
{
    /// <summary>
    /// Provides the header of the trace event block.
    /// </summary>
    internal sealed class TraceEventBlockHeader
    {
        /// <summary>
        /// Gets or sets length of the traces in that block
        /// </summary>
        public int DataLength
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets checksum of the length
        /// </summary>
        public int Checksum
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets start time of the traces
        /// </summary>
        public long StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets end time of the traces
        /// </summary>
        public long EndTime
        {
            get;
            set;
        }

        /// <summary>
        /// Check if the instance of TraceEventBlockHeader
        /// is a valid header
        /// </summary>
        /// <returns>
        /// returns a boolean indicating whether the header is valid
        /// </returns>
        public bool IsValid()
        {
            if (this.DataLength <= 0)
            {
                return false;
            }

            if ((this.Checksum ^ Constant.CheckSumKey) != this.DataLength)
            {
                return false;
            }

            if (this.StartTime > this.EndTime)
            {
                return false;
            }

            return true;
        }
    }
}
