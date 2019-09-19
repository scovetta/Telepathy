// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.EchoSvcLib
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Service contract for the echo service.
    /// </summary>
    [DataContract]
    public class StatisticInfo
    {
        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        [DataMember]
        public DateTime StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        [DataMember]
        public DateTime EndTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the task id.
        /// </summary>
        [DataMember]
        public int TaskId
        {
            get;
            set;
        }
    }
}
