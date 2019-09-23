// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Interface
{
    using System.ServiceModel;

    /// <summary>
    /// Message contract to pass broker unavailable signal for REST service
    /// </summary>
    [MessageContract]
    public class BrokerInstanceUnavailable
    {
        /// <summary>
        /// Represents the broker instance unavailable action
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const string Action = @"http://hpc.microsoft.com/BrokerInstanceUnavailable";

        /// <summary>
        /// Get or set the broker launcher EPR
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [MessageBodyMember]
        public string BrokerLauncherEpr { get; set; }

        /// <summary>
        /// Get or set a value indicates if broker node is down
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [MessageBodyMember]
        public bool IsBrokerNodeDown { get; set; }
    }
}
