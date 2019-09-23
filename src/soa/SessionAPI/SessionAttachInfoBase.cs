// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session
{
    /// <summary>
    /// Base class for information to attach to a session
    /// </summary>
    public abstract class SessionAttachInfoBase
    {
        /// <summary>
        /// headnode name
        /// </summary>
        private string headnode;

        /// <summary>
        /// Constructor of <see cref="SessionAttachInfoBase"/>
        /// </summary>
        /// <param name="headnode">
        ///   <para />
        /// </param>
        /// <param name="sessionId">
        ///   <para />
        /// </param>
        public SessionAttachInfoBase(string headnode, int sessionId)
        {
            this.headnode = headnode;
        }

        /// <summary>
        /// Gets the head node name
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public string Headnode
        {
            get
            {
                return this.headnode;
            }
        }
    }
}